using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using PipelineProcessor2.JsonTypes;
using PipelineProcessor2.Nodes;
using PipelineProcessor2.Nodes.Internal;
using PipelineProcessor2.Pipeline.Detectors;
using PipelineProcessor2.Pipeline.Exceptions;
using PipelineProcessor2.Plugin;
using PluginTypes;

namespace PipelineProcessor2.Pipeline
{
    public static class PipelineState
    {
        public static Dictionary<int, DependentNode> DependencyGraph => dependencyGraph;
        public static GraphNode[] ActiveNodes { get { return nodes; } }
        public static NodeLinkInfo[] ActiveLinks { get { return links; } }
        public static PipelineScheduler Scheduler => scheduler;

        private static GraphNode[] nodes;
        private static NodeLinkInfo[] links;
        private static readonly PipelineScheduler scheduler = new PipelineScheduler();
        public static string InputDirectory = "", OutputDirectory = "";

        private static Dictionary<int, DependentNode> dependencyGraph;
        private static InputData[] inputs;
        private static SpecialNodeData specialNodes;
        private static DataStore staticData = new DataStore(true);

        public static void UpdateActiveGraph(GraphNode[] graphNodes, NodeLinkInfo[] graphLinks)
        {
            links = graphLinks;
            nodes = StripUnusedNodes(graphNodes, graphLinks);

            BuildDependencyGraph(ActiveNodes, ActiveLinks);
            inputs = FindStartLocations();
            staticData = new DataStore(true);
            specialNodes = SpecialNodeSearch.CheckForSpecialNodes(dependencyGraph, staticData);
        }

        /// <summary>
        /// Start processing the active graph
        /// </summary>
        public static void Start()
        {
            if (inputs.Length == 0)
            {
                Console.WriteLine("No start locations could be found");
                return;
            }
            else Console.WriteLine("Starting Execution Preparation");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            //gather static data
            foreach (KeyValuePair<int, DependentNode> pair in dependencyGraph)
                if (PluginStore.isGeneratorPlugin(pair.Value.Type))
                {
                    IGeneratorPlugin plugin = PluginStore.getPlugin(pair.Value.Type) as IGeneratorPlugin;
                    staticData.StoreResults(plugin.StaticData(pair.Value.Value), pair.Key, true);
                }

            PipelineExecutor[] pipes = BuildPipelines();

            stopwatch.Stop();
            Console.WriteLine("Preparation duration: " + stopwatch.Elapsed + " ms");

            //start the first wave of dependencies
            Console.WriteLine("Starting Processing");
            Console.WriteLine("");
            for (int p = 0; p < pipes.Length; p++)
                for (int i = 0; i < inputs.Length; i++)
                    pipes[p].TriggerDependencies(inputs[i].nodeId);
        }

        /// <summary>
        /// Builds pipelines for data processing, will perform segregation of nodes to minimize node processing duplication
        /// </summary>
        /// <returns>prepared pipelines</returns>
        private static PipelineExecutor[] BuildPipelines()
        {
            if (specialNodes.SyncInformation.SyncNodes.Length == 0)
                return BuildLinearPipeline();

            //Build Pipelines with sync pipeline segregation

            int[] inputNodeIds = new int[inputs.Length];
            for (var i = 0; i < inputs.Length; i++) inputNodeIds[i] = inputs[i].nodeId;
            Dictionary<int, InputData[]> groupInputLookup = new Dictionary<int, InputData[]>();
            Dictionary<int, SyncSplitGroup> nodeToGroup = new Dictionary<int, SyncSplitGroup>();

            //find groups which have input nodes
            foreach (SyncSplitGroup group in specialNodes.SyncInformation.NodeGroups)
            {
                List<InputData> inputs = new List<InputData>();

                foreach (int nodeId in group.ControllingNodes)
                {
                    if (inputNodeIds.Contains(nodeId))
                    {
                        IInputPlugin plugin = PluginStore.getInputPlugin(dependencyGraph[nodeId].Type);
                        if (!group.Input)
                        {
                            // Gather initial quantity requirements
                            group.Input = true;
                            group.RequiredPipes = plugin.InputDataQuantity(NodeValueOrInput(nodeId));
                        }
                        else if (group.RequiredPipes != plugin.InputDataQuantity(NodeValueOrInput(nodeId)))
                            throw new InputPluginQuantityMismatchException();

                        inputs.Add(new InputData(nodeId, plugin));
                    }

                    nodeToGroup.Add(nodeId, group);
                }

                groupInputLookup.Add(group.SyncNodeId, inputs.ToArray());
            }

            //check group dependencies are valid
            foreach (SyncSplitGroup group in specialNodes.SyncInformation.NodeGroups)
            {
                if (group.SyncNodeId == -1) continue; //nodes outside of sync blocks

                foreach (int dependent in group.Dependents)
                {
                    SyncSplitGroup otherGroup = nodeToGroup[dependent];
                    if (otherGroup.Input)
                        throw new PipelineException("Co-dependent sync nodes with inputs are not supported");

                    if (group.linked == null) otherGroup.linked = group;
                    else throw new PipelineException("Multi-Sync segmentation is not supported");
                }
            }

            //create new pipelines
            bool postBuildLinkRequired = false;
            List<PipelineExecutor> executors = new List<PipelineExecutor>();
            foreach (SyncSplitGroup group in specialNodes.SyncInformation.NodeGroups)
            {
                if (group.linked != null) group.RequiredPipes = group.linked.RequiredPipes;
                else if (group.linked == null && !group.Input) group.RequiredPipes = 1;

                //Prepare pipelines
                if (group.linked == null)
                {
                    group.pipes = new PipelineExecutor[group.RequiredPipes];
                    for (int i = 0; i < group.RequiredPipes; i++)
                        group.pipes[i] = new PipelineExecutor(dependencyGraph, staticData, i, specialNodes,
                            InputDirectory, OutputDirectory);
                }
                else postBuildLinkRequired = true;

                if (group.Input) PrepareInputData(groupInputLookup[group.SyncNodeId], group.pipes);
                if (group.pipes != null) executors.AddRange(group.pipes);
            }

            if (postBuildLinkRequired)
            {
                foreach (SyncSplitGroup group in specialNodes.SyncInformation.NodeGroups)
                    if (group.linked != null && group.pipes == null) group.pipes = group.linked.pipes;
            }

            //Update sync nodes with trigger pipelines
            for (var i = 0; i < specialNodes.SyncInformation.NodeGroups.Count; i++)
            {
                SyncSplitGroup group = specialNodes.SyncInformation.NodeGroups[i];
                if (group.CalledBy == -2) continue;

                ExtractNodeSlot(group.CalledBy).StateInfo(group.pipes);
            }

            return executors.ToArray();
        }

        /// <summary>
        /// Builds a pipelines that is a consistent amount of pipelines throughout execution
        /// </summary>
        /// <returns>prepared pipelines</returns>
        private static PipelineExecutor[] BuildLinearPipeline()
        {
            int inputAmount = inputs[0].plugin.InputDataQuantity(NodeValueOrInput(inputs[0].nodeId));
            Console.WriteLine(inputAmount + " valid files found!");

            //Ensure consistent input amount
            for (int i = 1; i < inputs.Length; i++)
            {
                if (inputAmount != inputs[i].plugin.InputDataQuantity(NodeValueOrInput(inputs[i].nodeId)))
                    throw new InputPluginQuantityMismatchException();
            }

            //create a pipeline for each input data
            PipelineExecutor[] pipes = new PipelineExecutor[inputAmount];
            for (int i = 0; i < inputAmount; i++)
                pipes[i] = new PipelineExecutor(dependencyGraph, staticData, i, specialNodes, InputDirectory, OutputDirectory);
            foreach (SyncNode sync in specialNodes.SyncInformation.SyncNodes)
                sync.StateInfo(pipes);

            PrepareInputData(inputs, pipes);

            return pipes;
        }

        /// <summary>
        /// Takes input data and feeds it into pipelines
        /// </summary>
        /// <param name="inputData">input plugins</param>
        /// <param name="pipes">pipelines that will be filled with input data</param>
        private static void PrepareInputData(InputData[] inputData, PipelineExecutor[] pipes)
        {
            foreach (InputData data in inputData)
            {
                IEnumerable<List<byte[]>> enumerable = data.plugin.RetrieveData(NodeValueOrInput(data.nodeId));
                if (enumerable == null)
                    throw new NodeException("Input data could not be fully gathered due to input node issue in " + data.plugin.Name);

                int count = 0;
                foreach (List<byte[]> rawInputData in enumerable)
                {
                    pipes[count].StoreInputData(rawInputData, data.nodeId);
                    count++;
                }

                if (count != pipes.Length)
                    throw new InvalidDataException("Premature end of data from " + data.nodeId + ", " + data.plugin.Name + "!!");
            }
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

            if (nodes.Length == 0)
            {
                Console.WriteLine("No executable nodes found after unused node detection!");
                return;
            }

            //add valid nodes
            foreach (GraphNode node in nodes)
                if (!dependencyGraph.ContainsKey(node.id))
                    dependencyGraph.Add(node.id, new DependentNode(node));

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

        private static SyncNode ExtractNodeSlot(int nodeId)
        {
            foreach (SyncNode node in specialNodes.SyncInformation.SyncNodes)
                if (node.NodeId == nodeId) return node;
            return null;
        }

        private static SyncSplitGroup ExtractSyncGroup(int nodeId)
        {
            foreach (SyncSplitGroup node in specialNodes.SyncInformation.NodeGroups)
                if (node.SyncNodeId == nodeId) return node;
            return null;
        }

        private static InputData[] FindStartLocations()
        {
            List<InputData> inputNodes = new List<InputData>();

            foreach (KeyValuePair<int, DependentNode> node in dependencyGraph)
            {
                if (!PluginStore.isRegisteredPlugin(node.Value.Type))
                    throw new MissingPluginException(node.Value.Type + " could not be found");

                if (node.Value.Dependencies.Length == 0 && PluginStore.isInputPlugin(node.Value.Type))
                    inputNodes.Add(new InputData(node.Key, PluginStore.getInputPlugin(dependencyGraph[node.Key].Type)));
            }

            return inputNodes.ToArray();
        }

        private static string NodeValueOrInput(int nodeId)
        {
            return string.IsNullOrWhiteSpace(dependencyGraph[nodeId].Value) ?
                InputDirectory : dependencyGraph[nodeId].Value;
        }

#if DEBUG
        public static PipelineExecutor[] BuildPipesTestOnly()
        {
            return BuildPipelines();
        }

        public static SpecialNodeData SpecialNodeDataTestOnly => specialNodes;
#endif

        public static void ClearAll()
        {
            nodes = new GraphNode[0];
            links = new NodeLinkInfo[0];
        }

        private struct InputData
        {
            public int nodeId;
            public IInputPlugin plugin;

            public InputData(int nodeId, IInputPlugin plugin)
            {
                this.nodeId = nodeId;
                this.plugin = plugin;
            }
        }
    }
}
