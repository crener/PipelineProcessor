using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using PipelineProcessor2.Nodes.Internal;
using PipelineProcessor2.Pipeline.Exceptions;
using PipelineProcessor2.Plugin;

namespace PipelineProcessor2.Pipeline.Detectors
{
    public class LoopDetector
    {
        private const string StartEndIdMismatch = "Loop start and loop end only partly referencing each other!";

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
            DependentNode[] nodes = dependencyGraph.Values.ToArray();

            //sanity check for existing loops
            bool loopEnds = false;
            foreach (DependentNode node in nodes)
            {
                if (node.Type == LoopEnd.TypeName)
                {
                    loopEnds = true;
                    break;
                }
            }

            if (!loopEnds) return new List<LoopPair>();

            //find end points
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

                int startNode = ExecutionHelper.FindFirstNodeSlotInDependencies(loopEnd, dependencyGraph, 0).NodeId;
                if (startNode == -1) throw new MissingLinkException("No link for loop start specified");
                DependentNode loopStart = dependencyGraph[startNode];

                //try to keep one instance per unique loop start node Id
                if (loopStarts.ContainsKey(startNode))
                    instance.Start = loopStarts[startNode];
                else
                {
                    instance.Start = new LoopStart(loopStart);
                    loopStarts.Add(startNode, instance.Start);
                }

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
                    throw new SlotLimitExceeded("Loop Start may only have one loop end, use multiple loop starts for nesting or branching");

                    #region
                    //multiple nodes linked so there must be nested loops sharing this start position
                    /*int[] ids = new int[loopCount];
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
                    }*/
                    #endregion
                }
                else
                {
                    NodeSlot startLink = ExecutionHelper.FindFirstNodeSlotInDependents(loopStart, dependencyGraph, 0);

                    if (startLink.NodeId == -1)
                        throw new MissingLinkException("No link for loop start specified");
                    if (startLink.NodeId != loopEnd.Id)
                        throw new MissingLinkException(StartEndIdMismatch);

                    int foundEnd;
                    //contains internal loop?
                    if (ContainsLoop(instance, out foundEnd))
                        FindLoopPairs(dependencyGraph[foundEnd], ref foundEnds, instance.Depth + 1);

                    instance.ContainedNodes = FindContainingNodeIds(instance.Start.NodeId, instance.End.NodeId);

                    int start = 0, end = 0;
                    foreach (NodeSlot nodes in instance.ContainedNodes)
                    {
                        if (dependencyGraph[nodes.NodeId].Type == LoopStart.TypeName) start += 1;
                        else if (dependencyGraph[nodes.NodeId].Type == LoopEnd.TypeName) end += 1;
                    }
                    if (start != end) throw new CoDependentLoopException();

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
        /// Looks at the dependencies of a node and checks if any match the given types
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

        private List<NodeSlot> FindContainingNodeIds(int loopStart, int loopEnd)
        {
            DependentNode start = dependencyGraph[loopStart];
            List<NodeSlot> ids = new List<NodeSlot>();

            for (int i = 0; i < start.Dependents.Length; i++)
            {
                if (start.Dependents[i].NodeId == loopEnd) continue;

                List<NodeSlot> id = new List<NodeSlot>();
                id.Add(start.Dependents[i]);

                Tuple<bool, List<NodeSlot>> traversed = TraverseNodeDependents(id, start.Dependents[i].NodeId, loopEnd);
                if (traversed.Item1) ids.AddRange(traversed.Item2);
            }

            return ids;
        }

        private Tuple<bool, List<NodeSlot>> TraverseNodeDependents(List<NodeSlot> checkedNodes, int nextCheck, int endNode)
        {
            bool good = false;
            List<NodeSlot> validated = new List<NodeSlot>();

            foreach (NodeSlot node in dependencyGraph[nextCheck].Dependents)
            {
                if (node.NodeId == endNode) return new Tuple<bool, List<NodeSlot>>(true, checkedNodes);

                List<NodeSlot> searched = new List<NodeSlot>(checkedNodes);
                searched.Add(new NodeSlot(node.NodeId, node.SlotPos));

                Tuple<bool, List<NodeSlot>> result = TraverseNodeDependents(searched, node.NodeId, endNode);
                if (result.Item1)
                {
                    good = true;
                    validated.AddRange(result.Item2);
                }
            }

            return new Tuple<bool, List<NodeSlot>>(good, validated);
        }
    }
}
