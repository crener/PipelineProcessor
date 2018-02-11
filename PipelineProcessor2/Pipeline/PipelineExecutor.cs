using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PipelineProcessor2.Nodes.Internal;
using PipelineProcessor2.Pipeline.Detectors;
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

        private SpecialNodeData specialNodes;

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
            specialNodes = SpecialNodeSearch.CheckForSpecialNodes(nodes);

            inputDirectory = input;
            outputDirectory = output;
            run = depth;
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
            return specialNodes.Loops;
        }
#endif
    }
}
