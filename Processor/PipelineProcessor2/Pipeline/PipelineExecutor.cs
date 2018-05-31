using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineProcessor2.Nodes.Internal;
using PipelineProcessor2.Pipeline.Detectors;
using PipelineProcessor2.Plugin;

namespace PipelineProcessor2.Pipeline
{
    /// <summary>
    /// Executes a single instance of the pipeline until it runs out of data to process
    /// </summary>
    public class PipelineExecutor
    {
        private readonly Dictionary<int, DependentNode> dependencyGraph;
        private readonly DataStore data, staticData;
        private string inputDirectory, outputDirectory;
        private int depth;

        private Dictionary<int, LoopPair> loopPairByEnd;
        private Dictionary<int, LoopStart> loopPairByStart;
        private Dictionary<int, SyncNode> syncById;

        /// <summary>
        /// Initializes a standard Pipeline executor
        /// </summary>
        /// <param name="nodes">Nodes covered in the processing pipeline</param>
        /// <param name="staticData">Data that is generated once and remains static through multiple pipeline executors</param>
        /// <param name="specialNodes">Unique node information form the pipeline state</param>
        /// <param name="depth">the iteration of input that that is being processed</param>
        /// <param name="input">input path</param>
        /// <param name="output">output path</param>
        public PipelineExecutor(Dictionary<int, DependentNode> nodes, DataStore staticData, int depth, SpecialNodeData specialNodes, string input = "", string output = "")
        {
            dependencyGraph = nodes;
            data = new DataStore(depth, output);
            this.staticData = staticData;
            ExtractSpecialNodeData(specialNodes.SyncInformation.SyncNodes);

            inputDirectory = input;
            outputDirectory = output;
            this.depth = depth;
        }

        private void ExtractSpecialNodeData(SyncNode[] syncNodeBlocks)
        {
            //loops
            loopPairByStart = new Dictionary<int, LoopStart>();
            loopPairByEnd = new Dictionary<int, LoopPair>();

            List<LoopPair> loops = new LoopDetector(dependencyGraph).FindLoops();

            foreach (LoopPair loopPair in loops)
            {
                if (!loopPairByStart.ContainsKey(loopPair.Start.NodeId))
                    loopPairByStart.Add(loopPair.Start.NodeId, loopPair.Start);

                loopPairByEnd.Add(loopPair.End.NodeId, loopPair);
            }

            //sync
            syncById = new Dictionary<int, SyncNode>();
            foreach (SyncNode sync in syncNodeBlocks)
                syncById.Add(sync.NodeId, sync);
        }

        /// <summary>
        /// Triggers the dependencies of a node for execution
        /// </summary>
        /// <param name="targetId">Node that is needs its dependencies triggered</param>
        public void TriggerDependencies(int targetId)
        {
            //start the next set of tasks
            List<int> ids = new List<int>();
            DependentNode targetNode = dependencyGraph[targetId];
            for (int i = 0; i < targetNode.Dependents.Length; i++)
            {
                if (!ids.Contains(targetNode.Dependents[i].NodeId))
                    ids.Add(targetNode.Dependents[i].NodeId);
            }

            StartNodes(ids.ToArray(), targetId);

            GC.Collect();
        }

        /// <summary>
        /// Performs the task of an internalPlugin
        /// </summary>
        /// <param name="toTrigger">id of the plugin in the dependency graph</param>
        /// <param name="triggeredBy">the node which triggered the execution of this node</param>
        /// <returns>Should the dependencies be triggered</returns>
        private void InternalPluginAction(int toTrigger, int triggeredBy)
        {
            if (!ExecutionHelper.HasFulfilledDependency(dependencyGraph[toTrigger], data, staticData))
                return;

            if (dependencyGraph[toTrigger].Type == LoopStart.TypeName)
            {
                int[] startIds = loopPairByStart[toTrigger].StartDependencies(triggeredBy, data);
                StartNodes(startIds, toTrigger);
            }
            else if (dependencyGraph[toTrigger].Type == LoopEnd.TypeName)
            {
                int[] startIds = loopPairByEnd[toTrigger].End.Finished(data, dependencyGraph);
                StartNodes(startIds, toTrigger);
            }
            else if (dependencyGraph[toTrigger].Type == SyncNode.TypeName)
            {
                syncById[toTrigger].StoreData(data, triggeredBy);
            }
        }

        /// <summary>
        /// Queues nodes for execution
        /// </summary>
        /// <param name="nodes">node ids that will be triggered</param>
        /// <param name="triggeredBy">node that triggered the execution</param>
        private void StartNodes(int[] nodes, int triggeredBy)
        {
            foreach (int id in nodes)
            {
                string name = dependencyGraph[id].Type;

                //check if the node is a special type
                if (PluginStore.isInternalPlugin(name))
                {
                    InternalPluginAction(id, triggeredBy);
                    continue;
                }

                TaskRunner pluginTask = new TaskRunner(PluginStore.getPlugin(name), dependencyGraph[id], data, staticData, this, depth);

                Task task = pluginTask.getTask();
                if (task == null) continue;

                task.Start(PipelineState.Scheduler);
            }
        }

        internal void StoreInputData(List<byte[]> current, int inputId)
        {
            data.StoreResults(current, inputId, true);
        }

#if DEBUG
        public List<LoopPair> getLoops()
        {
            return new List<LoopPair>(loopPairByEnd.Values);
        }
#endif
    }
}
