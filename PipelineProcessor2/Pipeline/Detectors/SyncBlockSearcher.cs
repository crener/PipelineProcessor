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
        public static SyncNode[] FindSyncBlocks(Dictionary<int, DependentNode> dependencyGraph)
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
                            throw new InvalidConnectionException("Generators don't need to be synced as they are inherently identical");
                    }

                    syncBlocks.Add(new SyncNode(node));
                }
            }

            return syncBlocks.ToArray();
        }
    }
}
