using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PipelineProcessor2.Pipeline.Exceptions;
using PipelineProcessor2.PluginImporter;

namespace PipelineProcessor2.Pipeline
{
    public class PipelineExecutor
    {
        private readonly Dictionary<int, DependentNode> dependencyGraph;
        private readonly DataStore data;
        private string inputDirectory, outputDirectory;
        private int run;

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


            inputDirectory = input;
            outputDirectory = output;
            run = depth;
        }

        public void TriggerDependencies(int id)
        {
            Console.WriteLine("Finished node " + id + " of run " + run + " type " + dependencyGraph[id].Type + " starting dependencies");

            //start the first set of tasks
            foreach (NodeSlot slot in dependencyGraph[id].Dependents)
            {
                string name = dependencyGraph[slot.NodeId].Type;
                TaskRunner pluginTask = new TaskRunner(PluginStore.getPlugin(name), dependencyGraph[slot.NodeId], data, this);

                Task task = pluginTask.getTask();
                if (task == null) continue;

                Console.WriteLine("Starting node " + slot.NodeId + " of run " + run + " type " + dependencyGraph[slot.NodeId].Type);
                task.Start();
            }

            GC.Collect();
        }

        internal void StoreInputData(List<byte[]> current, int inputId)
        {
            data.StoreResults(current, inputId, true);
        }
    }
}
