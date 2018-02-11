using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PipelineProcessor2.Nodes.Internal;
using PipelineProcessor2.Pipeline.Exceptions;
using PipelineProcessor2.PluginImporter;

namespace PipelineProcessor2.Pipeline
{
    /// <summary>
    /// Executes a single instance of the pipeline until it runs out of data to process
    /// </summary>
    public class PipelineExecutor
    {
        private readonly Dictionary<int, DependentNode> dependencyGraph;
        private readonly DataStore data;
        private string inputDirectory, outputDirectory;
        private int run;

        //special nodes
        private List<LoopPair> loopPairs = new List<LoopPair>();

        private int loopIdCount = 0;

        /// <summary>
        /// Initializes a standard Pipeline executor
        /// </summary>
        /// <param name="nodes">Nodes covered in the processing pipeline</param>
        /// <param name="depth">the iteration of input that that is being processed</param>
        /// <param name="input">input path</param>
        /// <param name="output">output path</param>
        public PipelineExecutor(Dictionary<int, DependentNode> nodes, int depth, string input = "", string output = "")
        {
            dependencyGraph = nodes;
            data = new DataStore(depth, output);
            CheckForSpecialNodes();

            inputDirectory = input;
            outputDirectory = output;
            run = depth;
        }

        /// <summary>
        /// Initializes any special nodes that are used in the pipeline
        /// </summary>
        private void CheckForSpecialNodes()
        {
            List<int> done = new List<int>();

            //find end points
            DependentNode[] nodes = dependencyGraph.Values.ToArray();
            for (int i = nodes.Length - 1; i >= 0; i--)
            {
                DependentNode node = nodes[i];
                if (PluginStore.isOutputPlugin(node.Type))
                    FindLoopEnd(node, ref done); //FindLoopPairs(node, 0);
            }
        }

        private void FindLoopEnd(DependentNode node, ref List<int> done)
        {
            if (done.Contains(node.Id)) return;

            foreach (NodeSlot slot in node.Dependencies)
            {
                DependentNode testNode = dependencyGraph[slot.NodeId];
                if(done.Contains(testNode.Id)) continue;

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
                instance.End = new LoopEnd(loopEnd.Id);
                instance.Start = new LoopStart(loopEnd.Dependencies[0].NodeId);
                instance.Depth = depth;

                DependentNode loopStart = dependencyGraph[loopEnd.Dependencies[0].NodeId];

                if (loopStart.Dependents.Where((testNode, b) => testNode.SlotPos == 0 &&
                    dependencyGraph[testNode.NodeId].Type != LoopEnd.TypeName).Any())
                    throw new InvalidNodeException("Loop Start Link (slot 0) cannot link to anything but a Loop End");

                int loopCount = loopStart.Dependents.Where((testNode, b) => testNode.SlotPos == 0).Count();
                if (loopCount > 1)
                {
                    //multiple nodes linked so there must be nested loops sharing this start position
                    int[] ids = new int[loopCount];
                    for (int i = 0, c = 0; i < loopStart.Dependents.Length; i++)
                        if (loopStart.Dependents[i].SlotPos == 0)
                            ids[c++] = loopStart.Dependents[i].NodeId;

                    //check if co dependent
                    List<int> dependencies = new List<int>();
                    for(int a = 0; a < ids.Length; a++)
                    {
                        for(int b = 0; b < ids.Length; b++)
                        {
                            if(a == b) continue;

                            bool aTob = NodeDependentOn(ids[a], ids[b]),
                                bToa = NodeDependentOn(ids[b], ids[a]);

                            if (aTob && bToa) throw new CoDependentLoopException
                                    ("Node " + ids[a] + " and " + ids[b] + "Are dependent on each other and can't loop!");

                            if(aTob && !dependencies.Contains(a)) dependencies.Add(ids[a]);
                        }
                    }

                    //check non dependent nodes for internal loops
                    foreach(int id in ids)
                    {
                        if(dependencies.Contains(id)) continue;

                        LoopPair subPair = new LoopPair();
                        subPair.Id = loopIdCount++;
                        subPair.Depth = depth;
                        subPair.Start = new LoopStart(loopEnd.Dependencies[0].NodeId);
                        subPair.End = new LoopEnd(dependencyGraph[id].Id);

                        int foundEnd;
                        if (ContainsLoop(instance, out foundEnd))
                            //contains internal loop
                            FindLoopPairs(dependencyGraph[foundEnd], ref foundEnds, subPair.Depth + 1);

                        foundEnds.Add(subPair.End.NodeId);
                        loopPairs.Add(subPair);
                    }

                    //go back and handel the nodes which are dependent on each other
                    //todo
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
                if(checkedNodes.Contains(nodeId)) continue;

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

        private struct LoopDepthIds
        {
            public int startId, endId, depth;
        }

        private void BuildLoopPairs(List<LoopDepthIds> ends)
        {
            for (var i = 0; i < ends.Count; i++)
            {
                LoopStart start = new LoopStart(ends[i].startId);
                LoopEnd end = new LoopEnd(ends[i].endId);

                LoopPair pair = new LoopPair
                {
                    Start = start,
                    End = end,
                    Id = i,
                    Iteration = 0,
                    Depth = 0
                };

                loopPairs.Add(pair);
            }
        }


        public void TriggerDependencies(int id)
        {
            //check if the node is a special type
            if (PluginStore.isInternalPlugin(dependencyGraph[id].Type))
                if (!InternalPluginAction(id)) return;

            //start the first set of tasks
            foreach (NodeSlot slot in dependencyGraph[id].Dependents)
            {
                string name = dependencyGraph[slot.NodeId].Type;
                TaskRunner pluginTask = new TaskRunner(PluginStore.getPlugin(name), dependencyGraph[slot.NodeId], data, this, slot, run);

                Task task = pluginTask.getTask();
                if (task == null) continue;

                task.Start();
            }

            GC.Collect();
        }

        /// <summary>
        /// Performs the task of an internalPlugin
        /// </summary>
        /// <param name="id">id of the plugin in the dependency graph</param>
        /// <returns>Should the dependencies be triggered</returns>
        private bool InternalPluginAction(int id)
        {


            return true;
        }

        internal void StoreInputData(List<byte[]> current, int inputId)
        {
            data.StoreResults(current, inputId, true);
        }

#if DEBUG
        public List<LoopPair> getLoops()
        {
            return loopPairs;
        }
#endif

        /// <summary>
        /// Structure for holding all required 
        /// </summary>
#if DEBUG
        public struct LoopPair
#else
        private struct LoopPair
#endif
        {
            public LoopStart Start;
            public LoopEnd End;
            public int Iteration, Id, Depth;
        }
    }
}
