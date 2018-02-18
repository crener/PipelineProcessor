using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipelineProcessor2.Nodes.Internal;
using PipelineProcessor2.Pipeline.Exceptions;
using PipelineProcessor2.PluginImporter;

namespace PipelineProcessor2.Pipeline.Detectors
{
    internal class LoopDetector
    {
        private Dictionary<int, DependentNode> dependencyGraph;
        private List<LoopPair> loopPairs = new List<LoopPair>();
        private int loopIdCount = 0;
        private Dictionary<int, LoopStart> loopStarts = new Dictionary<int, LoopStart>();

        public LoopDetector(Dictionary<int, DependentNode> dependencyGraph)
        {
            this.dependencyGraph = dependencyGraph;
        }

        public List<LoopPair> FindLoops()
        {
            List<int> done = new List<int>();

            //find end points
            DependentNode[] nodes = dependencyGraph.Values.ToArray();
            for (int i = nodes.Length - 1; i >= 0; i--)
            {
                DependentNode node = nodes[i];
                if (PluginStore.isOutputPlugin(node.Type))
                    FindLoopEnd(node, ref done);
            }

            return loopPairs;
        }

        private void FindLoopEnd(DependentNode node, ref List<int> done)
        {
            if (done.Contains(node.Id)) return;

            foreach (NodeSlot slot in node.Dependencies)
            {
                DependentNode testNode = dependencyGraph[slot.NodeId];
                if (done.Contains(testNode.Id)) continue;

                if (testNode.Type == LoopEnd.TypeName) FindLoopPairs(testNode, ref done);
                else FindLoopEnd(testNode, ref done);
            }

            done.Add(node.Id);
        }

        private void FindLoopPairs(DependentNode loopEnd, ref List<int> foundEnds, int depth = 0)
        {
            LoopPair instance = new LoopPair();
            instance.Id = loopIdCount++;

            if (loopEnd.Type == LoopEnd.TypeName)
            {
                instance.End = new LoopEnd(loopEnd, instance);
                instance.Depth = depth;

                //try to keep one instance per unique loop start node Id
                if (loopStarts.ContainsKey(loopEnd.Dependencies[0].NodeId))
                    instance.Start = loopStarts[loopEnd.Dependencies[0].NodeId];
                else
                {
                    instance.Start = new LoopStart(dependencyGraph[loopEnd.Dependencies[0].NodeId]);
                    loopStarts.Add(loopEnd.Dependencies[0].NodeId, instance.Start);
                }

                DependentNode loopStart = dependencyGraph[loopEnd.Dependencies[0].NodeId];

                //ensure that only the start and end nodes are linked
                if (loopStart.Dependents.Where((testSlot, b) =>
                {
                    bool incorrectType = dependencyGraph[testSlot.NodeId].Type != LoopEnd.TypeName;

                    foreach (NodeSlot slot in dependencyGraph[testSlot.NodeId].Dependencies)
                        if (slot.NodeId == loopStart.Id && slot.SlotPos == 0)
                            if (incorrectType) return true;

                    return false;
                }).Any())
                    throw new InvalidNodeException("Loop Start Link (slot 0) cannot link to anything but a Loop End");

                //find out how many loops the "LoopStart" node is directly responsible for
                int loopCount;
                {
                    List<int> matched = new List<int>();
                    loopCount = loopStart.Dependents.Where((testSlot, b) =>
                    {
                        if (matched.Contains(testSlot.NodeId)) return false;

                        foreach (NodeSlot slot in dependencyGraph[testSlot.NodeId].Dependencies)
                            if (slot.NodeId == loopStart.Id && slot.SlotPos == 0)
                            {
                                matched.Add(testSlot.NodeId);
                                return true;
                            }

                        return false;
                    }).Count();
                }

                if (loopCount > 1)
                {
                    throw new NotImplementedException();

                    //multiple nodes linked so there must be nested loops sharing this start position
                    int[] ids = new int[loopCount];
                    for (int i = 0, c = 0; i < loopStart.Dependents.Length; i++)
                        if (loopStart.Dependents[i].SlotPos == 0)
                            ids[c++] = loopStart.Dependents[i].NodeId;

                    //check if co dependent nodes exist
                    List<int> dependencies = new List<int>();
                    for (int a = 0; a < ids.Length; a++)
                    {
                        for (int b = 0; b < ids.Length; b++)
                        {
                            if (a == b) continue;

                            bool aTob = NodeDependentOn(ids[a], ids[b]),
                                bToa = NodeDependentOn(ids[b], ids[a]);

                            if (aTob && bToa) throw new CoDependentLoopException
                                ("Node " + ids[a] + " and " + ids[b] + "Are dependent on each other and can't loop!");

                            if (aTob && !dependencies.Contains(a)) dependencies.Add(ids[a]);
                        }
                    }

                    //check non dependent nodes for internal loops
                    foreach (int id in ids)
                    {
                        if (dependencies.Contains(id)) continue;

                        LoopPair subPair = new LoopPair();
                        subPair.Id = loopIdCount++;
                        subPair.Depth = depth;
                        subPair.End = new LoopEnd(dependencyGraph[id], subPair);
                        if (loopStarts.ContainsKey(loopEnd.Dependencies[0].NodeId))
                            subPair.Start = loopStarts[loopEnd.Dependencies[0].NodeId];
                        else
                        {
                            instance.Start = new LoopStart(dependencyGraph[loopEnd.Dependencies[0].NodeId]);
                            loopStarts.Add(loopEnd.Dependencies[0].NodeId, subPair.Start);
                        }

                        int foundEnd;
                        if (ContainsLoop(instance, out foundEnd))
                            //contains internal loop
                            FindLoopPairs(dependencyGraph[foundEnd], ref foundEnds, subPair.Depth + 1);

                        foundEnds.Add(subPair.End.NodeId);
                        subPair.Start.AddLoopPair(ref subPair);
                        loopPairs.Add(subPair);
                    }

                    //go back and handle the nodes which are dependent on each other
                    if (dependencies.Count > 1)
                    {
                        throw new NotImplementedException();
                        //todo
                    }
                }
                else
                {
                    NodeSlot startLink;
                    try { startLink = loopStart.Dependents.First(nodeSlot => nodeSlot.SlotPos == 0); }
                    catch (Exception ex) { throw new MissingLinkException("No link for loop start specified", ex); }

                    if (startLink.NodeId != loopEnd.Id)
                        throw new MissingLinkException("Loop start and loop end only partly referencing each other!");

                    int foundEnd;
                    if (ContainsLoop(instance, out foundEnd))
                        //contains internal loop
                        FindLoopPairs(dependencyGraph[foundEnd], ref foundEnds, instance.Depth + 1);

                    foundEnds.Add(instance.End.NodeId);
                    instance.Start.AddLoopPair(ref instance);
                    loopPairs.Add(instance);
                }
            }
            else if (loopEnd.Type == LoopStart.TypeName)
                //abort as you are in the middle of a loop rather than at the end
                if (instance.End == null) return;
        }

        private bool ContainsLoop(LoopPair instance, out int foundId)
        {
            foundId = -1;
            List<int> ignore = new List<int>();
            ignore.AddRange(new[] { instance.Start.NodeId });

            if (CheckDependenciesFor(dependencyGraph[instance.End.NodeId], out foundId, ignore,
                LoopEnd.TypeName))
            {
                if (foundId != instance.Start.NodeId)
                    //must have an internal loop
                    return true;

                return false;
            }
            return false;
        }

        private bool NodeDependentOn(int search, int target)
        {
            DependentNode node = dependencyGraph[search];
            List<int> checkedNodes = new List<int>();

            for (int i = 0; i < node.Dependencies.Length; i++)
            {
                int nodeId = node.Dependencies[i].NodeId;
                if (checkedNodes.Contains(nodeId)) continue;

                if (nodeId == target) return true;
                if (NodeDependentOn(nodeId, target, ref checkedNodes)) return true;

                checkedNodes.Add(nodeId);
            }

            return false;
        }

        private bool NodeDependentOn(int search, int target, ref List<int> checkedNodes)
        {
            DependentNode node = dependencyGraph[search];

            for (int i = 0; i < node.Dependencies.Length; i++)
            {
                int nodeId = node.Dependencies[i].NodeId;
                if (checkedNodes.Contains(nodeId)) continue;

                if (nodeId == target && NodeDependentOn(nodeId, target, ref checkedNodes)) return true;
                if (NodeDependentOn(nodeId, target, ref checkedNodes)) return true;

                checkedNodes.Add(nodeId);
            }

            return false;
        }

        /// <summary>
        /// Looks at the dependencies of a node and checks if any 
        /// </summary>
        /// <param name="node">search node</param>
        /// <param name="matchedNode">id of the node that was of the given types, -1 if no match is found</param>
        /// <param name="tested">list of ids that have been checked</param>
        /// <param name="types">the node types that will be looked for</param>
        /// <returns>true if the node has a dependency of 'types'</returns>
        private bool CheckDependenciesFor(DependentNode node, out int matchedNode, List<int> tested, params string[] types)
        {
            if (tested.Contains(node.Id))
            {
                matchedNode = -1;
                return false;
            }

            foreach (NodeSlot slot in node.Dependencies)
            {
                DependentNode testNode = dependencyGraph[slot.NodeId];

                for (int i = 0; i < types.Length; i++)
                    if (testNode.Type == types[i] && !tested.Contains(testNode.Id))
                    {
                        matchedNode = testNode.Id;
                        return true;
                    }

                if (CheckDependenciesFor(testNode, out matchedNode, tested, types)) return true;
            }

            tested.Add(node.Id);
            matchedNode = -1;
            return false;
        }
    }
}
