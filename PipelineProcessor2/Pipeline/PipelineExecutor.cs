using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PipelineProcessor2.JsonTypes;
using PipelineProcessor2.Nodes;
using PipelineProcessor2.PluginImporter;

namespace PipelineProcessor2.Pipeline
{
    public class PipelineExecutor
    {
        public Dictionary<int, DependentNode> DependencyGraph => dependencyGraph;

        private Dictionary<int, DependentNode> dependencyGraph;
        private int[] inputIds;
        private IEnumerator<List<byte[]>>[] dataInputs;
        private string inputData, outputData;
        private DataStore data = new DataStore();

        public PipelineExecutor(string input = "", string output = "")
        {
            BuildDependencyGraph(PipelineState.ActiveNodes, PipelineState.ActiveLinks);
            inputIds = FindStartLocations();

            inputData = input;
            outputData = output;
        }

        public PipelineExecutor(GraphNode[] nodes, NodeLinkInfo[] links, string input = "", string output = "")
        {
            BuildDependencyGraph(nodes, links);
            inputIds = FindStartLocations();

            inputData = input;
            outputData = output;
        }

        public void Start()
        {
            if (inputIds.Length == 0)
            {
                Console.WriteLine("No start locations could be found");
                return;
            }

            IInputPlugin[] inputPlugins = new IInputPlugin[inputIds.Length];
            for (var i = 0; i < inputIds.Length; i++)
                inputPlugins[i] = PluginStore.getInputPlugin(dependencyGraph[inputIds[i]].Type);

            if (inputIds.Length > 1)
            {
                //since there are multiple start locations make sure that there is an 
                //equal amount of data to process for each one

                int inputAmount = inputPlugins[0].InputDataQuantity(inputData);

                for (int i = 1; i < inputIds.Length; i++)
                {
                    int pluginAmount = inputPlugins[i].InputDataQuantity(inputData);
                    if (inputAmount != pluginAmount) throw new InputPluginQuantityMismatchException();
                }
            }

            //get the enumerators for the data and populate starting data
            dataInputs = new IEnumerator<List<byte[]>>[inputIds.Length];
            bool quit = false;
            for (var i = 0; i < inputIds.Length; i++)
            {
                dataInputs[i] = inputPlugins[i].RetrieveData(inputData).GetEnumerator();
                dataInputs[i].MoveNext();

                if (dataInputs[i].Current == null)
                {
                    Console.WriteLine("No input data from " +
                                      inputPlugins[i].PluginInformation(PluginInformationRequests.Name, 0));
                    quit = true;
                }
                else data.StoreResults(dataInputs[i].Current, inputIds[i]);
            }

            if (quit) return;
            
            //start the first set of tasks
            List<int> started = new List<int>();
            for (int i = 0; i < inputIds.Length; i++)
            {
                foreach (NodeSlot slot in dependencyGraph[inputIds[i]].Dependents)
                {
                    if(started.Contains(slot.NodeId)) continue;

                    string name = dependencyGraph[slot.NodeId].Type;
                    TaskRunner pluginTask = new TaskRunner(PluginStore.getPlugin(name), dependencyGraph[slot.NodeId], data);

                    Task task = pluginTask.getTask();
                    if(task == null) continue;
                    task.Start();

                    started.Add(slot.NodeId);
                }
            }
        }

        private void BuildDependencyGraph(GraphNode[] nodes, NodeLinkInfo[] links)
        {
            dependencyGraph = new Dictionary<int, DependentNode>();
            foreach (GraphNode node in nodes)
                if (!DependencyGraph.ContainsKey(node.id))
                    DependencyGraph.Add(node.id, new DependentNode(node.id, node.type));

            foreach (NodeLinkInfo info in links)
            {
                DependencyGraph[info.OriginId].AddDependent(info.TargetId, info.TargetSlot, info.OriginSlot);
                DependencyGraph[info.TargetId].AddDependency(info.OriginId, info.OriginSlot, info.TargetSlot);
            }
        }

        private int[] FindStartLocations()
        {
            List<int> inputNodes = new List<int>();

            foreach (KeyValuePair<int, DependentNode> node in dependencyGraph)
            {
                if (!PluginStore.isRegisteredPlugin(node.Value.Type))
                    throw new MissingPluginException(node.Value.Type + " could not be found");

                if (node.Value.Dependencies.Length == 0 && PluginStore.isInputPlugin(node.Value.Type))
                    inputNodes.Add(node.Key);
            }

            return inputNodes.ToArray();
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
            if (dependencies.ContainsKey(targetSlot)) throw new DataSlotAlreadyInUse("Slot " + targetSlot + " of node " + Id + "has already need assigned");

            dependencies.Add(targetSlot, nodeSlot);
        }

        public void AddDependent(int targetId, int targetSlot, int originSlot)
        {
            NodeSlot nodeSlot = new NodeSlot(targetId, targetSlot);

            dependents.Add(originSlot, nodeSlot);
        }
    }
}
