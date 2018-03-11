using System;
using System.Collections.Generic;
using System.Linq;
using PipelineProcessor2.JsonTypes;
using PipelineProcessor2.Nodes;
using PipelineProcessor2.Nodes.Internal;
using PipelineProcessor2.Pipeline.Detectors;
using PipelineProcessor2.Pipeline.Exceptions;
using PipelineProcessor2.Plugin;

namespace PipelineProcessor2.Pipeline
{
    public static class PipelineState
    {
        public static Dictionary<int, DependentNode> DependencyGraph => dependencyGraph;

        public static GraphNode[] ActiveNodes { get { return nodes; } }
        public static NodeLinkInfo[] ActiveLinks { get { return links; } }

        private static GraphNode[] nodes;
        private static NodeLinkInfo[] links;
        public static string InputDirectory = "", OutputDirectory = "";

        private static Dictionary<int, DependentNode> dependencyGraph;
        private static int[] inputIds;
        private static SyncNode[] syncNodeNodes;
        private static DataStore staticData = new DataStore(true);

        public static void UpdateActiveGraph(GraphNode[] graphNodes, NodeLinkInfo[] graphLinks)
        {
            links = graphLinks;
            nodes = StripUnusedNodes(graphNodes, graphLinks);

            BuildDependencyGraph(ActiveNodes, ActiveLinks);
            inputIds = FindStartLocations();
            staticData = new DataStore(true);
            syncNodeNodes = SyncBlockSearcher.PrepareSyncBlocks(dependencyGraph, staticData);
        }

        public static void Start()
        {
            if (inputIds.Length == 0)
            {
                Console.WriteLine("No start locations could be found");
                return;
            }
            else Console.WriteLine("Starting Processing...");

            IInputPlugin[] inputPlugins = new IInputPlugin[inputIds.Length];
            for (var i = 0; i < inputIds.Length; i++)
                inputPlugins[i] = PluginStore.getInputPlugin(dependencyGraph[inputIds[i]].Type);

            int inputAmount = inputPlugins[0].InputDataQuantity(InputDirectory);
            Console.WriteLine(inputAmount + " valid files found!");

            if (inputIds.Length > 1)
            {
                //Ensure that each start has an equal amount of outputs
                for (int i = 1; i < inputIds.Length; i++)
                {
                    int pluginAmount = inputPlugins[i].InputDataQuantity(InputDirectory);
                    if (inputAmount != pluginAmount) throw new InputPluginQuantityMismatchException();
                }
            }

            //gather static data
            foreach (KeyValuePair<int, DependentNode> pair in dependencyGraph)
            {
                if (PluginStore.isGeneratorPlugin(pair.Value.Type))
                {
                    IGeneratorPlugin plugin = PluginStore.getPlugin(pair.Value.Type) as IGeneratorPlugin;
                    staticData.StoreResults(plugin.StaticData(), pair.Key, true);
                }
            }

            //create a pipeline executor for each 
            PipelineExecutor[] pipes = new PipelineExecutor[inputAmount];
            for (int i = 0; i < inputAmount; i++)
                pipes[i] = new PipelineExecutor(dependencyGraph, staticData, syncNodeNodes, i, InputDirectory, OutputDirectory);

            //get the enumerators for the data and populate starting data
            IEnumerator<List<byte[]>>[] dataInputs = new IEnumerator<List<byte[]>>[inputIds.Length];
            bool quit = false;
            for (var i = 0; i < inputIds.Length; i++)
            {
                dataInputs[i] = inputPlugins[i].RetrieveData(InputDirectory).GetEnumerator();
                if (dataInputs[i] == null) quit = true;
            }

            if (quit)
            {
                Console.WriteLine("Input data could not be fully gathered due to input node issue");
                return;
            }

            //store the input data
            for (int p = 0; p < pipes.Length; p++)
                for (int d = 0; d < dataInputs.Length; d++)
                {
                    if (!dataInputs[d].MoveNext())
                    {
                        Console.WriteLine("Premature end of data!!");
                        break;
                    }
                    pipes[p].StoreInputData(dataInputs[d].Current, inputIds[d]);
                }

            //share the pipelines with the sync nodes
            foreach (SyncNode sync in syncNodeNodes)
                sync.StateInfo(pipes);

            //Dispose of the input enumerators
            for (int i = 0; i < dataInputs.Length; i++)
                dataInputs[i].Dispose();
            dataInputs = null;
            GC.Collect();

            //start the first wave of dependencies
            Console.WriteLine("Starting processing tasks");
            Console.WriteLine("");
            for (int p = 0; p < pipes.Length; p++)
                for (int i = 0; i < inputIds.Length; i++)
                    pipes[p].TriggerDependencies(inputIds[i]);
        }

        private static GraphNode[] StripUnusedNodes(GraphNode[] graphNodes, NodeLinkInfo[] graphLinks)
        {
            List<int> usedIds = new List<int>();
            foreach (NodeLinkInfo link in graphLinks)
            {
                if (!usedIds.Contains(link.OriginId)) usedIds.Add(link.OriginId);
                if (!usedIds.Contains(link.TargetId)) usedIds.Add(link.TargetId);
            }

            Dictionary<int, GraphNode> output = new Dictionary<int, GraphNode>((int)(graphNodes.Length * 1.4f));
            foreach (GraphNode node in graphNodes)
                if (usedIds.Contains(node.id)) output.Add(node.id, node);

            //ensure all links connections have been satisfied
            foreach (int id in usedIds)
                if (!output.ContainsKey(id))
                    throw new MissingNodeException(id + " is used by a link but does not have a node defined!");

            return output.Values.ToArray();
        }

        private static void BuildDependencyGraph(GraphNode[] nodes, NodeLinkInfo[] links)
        {
            dependencyGraph = new Dictionary<int, DependentNode>();

            //add valid nodes
            foreach (GraphNode node in nodes)
                if (!dependencyGraph.ContainsKey(node.id))
                    dependencyGraph.Add(node.id, new DependentNode(node.id, node.type));

            //setup dependencies
            foreach (NodeLinkInfo info in links)
            {
                if (dependencyGraph.ContainsKey(info.OriginId) &&
                    dependencyGraph.ContainsKey(info.TargetId))
                {
                    //todo check if slot types are compatible with the actual plugin

                    //todo check if a node has X slot before adding

                    dependencyGraph[info.OriginId].AddDependent(info.TargetId, info.TargetSlot, info.OriginSlot);
                    dependencyGraph[info.TargetId].AddDependency(info.OriginId, info.OriginSlot, info.TargetSlot);
                }
            }
        }

        private static int[] FindStartLocations()
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

        public static void ClearAll()
        {
            nodes = new GraphNode[0];
            links = new NodeLinkInfo[0];
        }
    }
}
