using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PipelineProcessor2.PluginImporter;

namespace PipelineProcessor2.Pipeline
{
    public class PipelineExecutor
    {
        private readonly Dictionary<int, DependentNode> dependencyGraph;
        private readonly DataStore data = new DataStore();
        private string inputDirectory, outputDirectory;
        private int run;

        public PipelineExecutor(Dictionary<int, DependentNode> nodes, int depth, string input = "", string output = "")
        {
            dependencyGraph = nodes;

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
            data.StoreResults(current, inputId);
        }
    }

    public class DependentNode
    {
        /// <summary>
        /// Nodes which require the output of this node to start
        /// </summary>
        public NodeSlot[] Dependents
        {
            get
            {
                NodeSlot[] deps = new NodeSlot[dependents.Count];
                foreach (KeyValuePair<int, NodeSlot> dependent in dependents)
                    deps[dependent.Key] = dependent.Value;

                return deps;
            }
        }

        /// <summary>
        /// Nodes which this node requires to start
        /// </summary>
        public NodeSlot[] Dependencies
        {
            get
            {
                NodeSlot[] deps = new NodeSlot[dependencies.Count];
                foreach (KeyValuePair<int, NodeSlot> dependency in dependencies)
                    deps[dependency.Key] = dependency.Value;

                return deps;
            }
        }

        public int Id { get; private set; }
        public string Type { get; private set; }

        //private List<NodeSlot> dependents, dependencies;
        private Dictionary<int, NodeSlot> dependents, dependencies;

        public DependentNode(int id, string type)
        {
            Id = id;
            Type = type;

            dependents = new Dictionary<int, NodeSlot>();
            dependencies = new Dictionary<int, NodeSlot>();
        }

        public void AddDependency(int originId, int originSlot, int targetSlot)
        {
            NodeSlot nodeSlot = new NodeSlot(originId, originSlot);
            if (dependencies.ContainsKey(targetSlot))
                throw new DataSlotAlreadyInUse("Slot " + targetSlot + " of node " + Id + " has already need assigned");

            dependencies.Add(targetSlot, nodeSlot);
        }

        public void AddDependent(int targetId, int targetSlot, int originSlot)
        {
            NodeSlot nodeSlot = new NodeSlot(targetId, targetSlot);
            if (dependents.ContainsKey(originSlot))
                throw new DataSlotAlreadyInUse("Slot " + targetSlot + " of node " + Id + " has already need assigned");

            dependents.Add(originSlot, nodeSlot);
        }
    }
}
