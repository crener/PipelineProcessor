using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipelineProcessor2.Nodes.Internal;
using PipelineProcessor2.Pipeline.Exceptions;
using PipelineProcessor2.Plugin;

namespace PipelineProcessor2.Pipeline.Detectors
{
    public static class SyncBlockSearcher
    {
        /// <summary>
        /// Searches for sync blocks in the graph and initializes them in preparation for execution
        /// </summary>
        /// <param name="dependencyGraph">graph representation of the pipeline</param>
        /// <param name="staticData">data object used by all pipelines for initialization of sync blocks</param>
        /// <returns>array of initialized sync blocks</returns>
        public static SyncNode[] PrepareSyncBlocks(Dictionary<int, DependentNode> dependencyGraph, DataStore staticData)
        {
            List<SyncNode> syncBlocks = new List<SyncNode>();

            foreach(DependentNode node in dependencyGraph.Values)
            {
                if(node.Type == SyncNode.TypeName)
                {
                    //check if the node is connected to a generator
                    foreach(NodeSlot dependencies in node.Dependencies)
                    {
                        if(PluginStore.isGeneratorPlugin(dependencyGraph[dependencies.NodeId].Type))
                            throw new InvalidConnectionException("Generators don't need to be synced as they are inherently consistant");
                    }

                    syncBlocks.Add(new SyncNode(node, staticData));
                }
            }

            return syncBlocks.ToArray();
        }
    }
}
