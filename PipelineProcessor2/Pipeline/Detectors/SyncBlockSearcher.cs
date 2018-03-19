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
        public static SyncData GatherData(Dictionary<int, DependentNode> dependencyGraph, DataStore staticData)
        {
            return new SyncData
            {
                SyncNodes = PrepareSyncBlocks(dependencyGraph, staticData),
                NodeGroups = NodeGroups(dependencyGraph)
            };
        }

        /// <summary>
        /// Searches for sync blocks in the graph and initializes them in preparation for execution
        /// </summary>
        /// <param name="dependencyGraph">graph representation of the pipeline</param>
        /// <param name="staticData">data object used by all pipelines for initialization of sync blocks</param>
        /// <returns>array of initialized sync blocks</returns>
        private static SyncNode[] PrepareSyncBlocks(Dictionary<int, DependentNode> dependencyGraph, DataStore staticData)
        {
            List<SyncNode> syncBlocks = new List<SyncNode>();

            foreach (DependentNode node in dependencyGraph.Values)
            {
                if (node.Type == SyncNode.TypeName)
                {
                    //check if the node is connected to a generator
                    foreach (NodeSlot dependencies in node.Dependencies)
                    {
                        if (PluginStore.isGeneratorPlugin(dependencyGraph[dependencies.NodeId].Type))
                            throw new InvalidConnectionException("Generators don't need to be synced as they are inherently consistent");
                    }

                    syncBlocks.Add(new SyncNode(node, staticData));
                }
            }

            return syncBlocks.ToArray();
        }

        private static List<SyncSplitGroup> NodeGroups(Dictionary<int, DependentNode> dependencyGraph)
        {
            List<SyncSplitGroup> sync = new List<SyncSplitGroup>();
            List<int> syncChecked = new List<int>();

            //find sync node to check
            foreach (DependentNode depNode in dependencyGraph.Values)
            {
                if (depNode.Type == SyncNode.TypeName && !syncChecked.Contains(depNode.Id))
                {
                    syncChecked.Add(depNode.Id);
                    break;
                }
            }

            if (syncChecked.Count == 0) return null;

            //identify and collect sync blocks
            foreach (int syncNode in syncChecked)
            {
                SyncSplitGroup group = new SyncSplitGroup();
                group.SyncNodeId = syncNode;
                group.ControllingNodes = new List<int>();
                DependentNode syncInstance = dependencyGraph[syncNode];

                //gather all dependencies
                foreach (NodeSlot node in syncInstance.Dependencies)
                {
                    if (group.ControllingNodes.Contains(node.NodeId)) continue;
                    if (dependencyGraph[node.NodeId].Type == SyncNode.TypeName) continue;
                    group.ControllingNodes.Add(node.NodeId);

                    AggregateNodeDependencies(dependencyGraph, dependencyGraph[node.NodeId], ref group.ControllingNodes);
                }

                sync.Add(group);
            }

            //check if a groups are interdependent
            for (int index = 0; index < sync.Count; index++)
            {
                SyncSplitGroup splitGroup = sync[index];
                List<int> dependents = new List<int>();

                foreach (int nodeId in splitGroup.ControllingNodes)
                {
                    DependentNode node = dependencyGraph[nodeId];
                    foreach (NodeSlot dependent in node.Dependents)
                    {
                        if (dependent.NodeId == splitGroup.SyncNodeId ||
                           splitGroup.ControllingNodes.Contains(dependent.NodeId))
                            continue;

                        dependents.Add(dependent.NodeId);
                    }
                }

                splitGroup.Dependents = dependents.ToArray();
                sync[index] = splitGroup;
            }

            return sync;
        }

        private static void AggregateNodeDependencies(Dictionary<int, DependentNode> nodes, DependentNode search, ref List<int> gathered)
        {
            foreach (NodeSlot dependent in search.Dependencies)
            {
                if (gathered.Contains(dependent.NodeId)) continue;
                if (nodes[dependent.NodeId].Type == SyncNode.TypeName) continue;

                gathered.Add(dependent.NodeId);
                AggregateNodeDependencies(nodes, nodes[dependent.NodeId], ref gathered);
            }
        }
    }
}
