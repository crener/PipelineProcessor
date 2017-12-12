using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private int[] startLocations;
        private IEnumerator[] dataInputs;

        public PipelineExecutor()
        {
            BuildDependencyGraph(PipelineState.ActiveNodes, PipelineState.ActiveLinks);
            startLocations = FindStartLocations();
        }

        public PipelineExecutor(GraphNode[] nodes, NodeLinkInfo[] links)
        {
            BuildDependencyGraph(nodes, links);
            startLocations = FindStartLocations();
        }

        public void Start()
        {
            IInputPlugin[] plugins = new IInputPlugin[startLocations.Length];
            for (var i = 0; i < startLocations.Length; i++)
                plugins[i] = PluginStore.getInputPlugin(dependencyGraph[i].Type);

            if (startLocations.Length > 1)
            {
                //since there are multiple start locations make sure that there is an 
                //equal amount of data to process for each one

                int inputAmount = plugins[0].InputDataQuantity("");

                for (int i = 1; i < startLocations.Length; i++)
                {
                    int pluginAmount = plugins[i].InputDataQuantity("");
                    if (inputAmount != pluginAmount) throw new InputPluginQuantityMismatchException();
                }
            }

            //get the enumerators for the data
            dataInputs = new IEnumerator[startLocations.Length];
            for (var i = 0; i < startLocations.Length; i++)
                dataInputs[i] = plugins[i].RetrieveData("").GetEnumerator();


        }

        private void BuildDependencyGraph(GraphNode[] nodes, NodeLinkInfo[] links)
        {
            dependencyGraph = new Dictionary<int, DependentNode>();
            foreach (GraphNode node in nodes)
                if (!DependencyGraph.ContainsKey(node.id))
                    DependencyGraph.Add(node.id, new DependentNode(node.id, node.type));

            foreach (NodeLinkInfo info in links)
            {
                DependencyGraph[info.OriginId].AddDependent(info.TargetId);
                DependencyGraph[info.TargetId].AddDependency(info.OriginId);
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
        public int[] Dependents => dependents.ToArray();

        /// <summary>
        /// Nodes which this node requires to start
        /// </summary>
        public int[] Dependencies => dependencies.ToArray();
        public int Id { get; private set; }
        public string Type { get; private set; }

        private List<int> dependents, dependencies;

        public DependentNode(int id, string type)
        {
            Id = id;
            Type = type;

            dependents = new List<int>();
            dependencies = new List<int>();
        }

        public void AddDependency(int id)
        {
            if (!dependencies.Contains(id)) dependencies.Add(id);
        }

        public void AddDependent(int id)
        {
            if (!dependents.Contains(id)) dependents.Add(id);
        }
    }
}
