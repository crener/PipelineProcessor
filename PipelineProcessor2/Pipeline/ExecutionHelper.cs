using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineProcessor2.Pipeline
{
    public static class ExecutionHelper
    {
        public static bool HasFulfilledDependency(DependentNode node, DataStore pipelineData, DataStore staticData)
        {
            foreach (NodeSlot id in node.Dependencies)
                if (pipelineData.getData(id) == null && staticData.getData(id) == null)
                    return false;

            return true;
        }

        public static int OtherNodeSlotDependents(DependentNode node, int targetNodeId)
        {
            foreach(NodeSlot slot in node.Dependents)
                if(slot.NodeId == targetNodeId) return slot.SlotPos;

            return -1;
        }

        public static int OtherNodeSlotDependencies(DependentNode node, int targetNodeId)
        {
            foreach(NodeSlot slot in node.Dependencies)
                if(slot.NodeId == targetNodeId) return slot.SlotPos;

            return -1;
        }

        /// <summary>
        /// Returns the nodeSlot that is linked to the search node's given slot in its dependents
        /// </summary>
        /// <param name="searchNode">node to look through</param>
        /// <param name="dependencyGraph">nodes in graph</param>
        /// <param name="searchSlot">slot that other node links to</param>
        /// <returns>node slot that points to the node that is linked to the search nodes search slot</returns>
        public static NodeSlot FindFirstNodeSlotInDependents(DependentNode searchNode, Dictionary<int, DependentNode> dependencyGraph, int searchSlot)
        {
            foreach (NodeSlot slot in searchNode.Dependents)
                if (OtherNodeSlotDependencies(dependencyGraph[slot.NodeId], searchNode.Id) == searchSlot)
                    return slot;

            return new NodeSlot(-1, -1);
        }

        /// <summary>
        /// Returns the nodeSlot that is linked to the search node's given slot in its dependencies
        /// </summary>
        /// <param name="searchNode">node to look through</param>
        /// <param name="dependencyGraph">nodes in graph</param>
        /// <param name="searchSlot">slot that other node links to</param>
        /// <returns>node slot that points to the node that is linked to the search nodes search slot</returns>
        public static NodeSlot FindFirstNodeSlotInDependencies(DependentNode searchNode, Dictionary<int, DependentNode> dependencyGraph, int searchSlot)
        {
            foreach (NodeSlot slot in searchNode.Dependencies)
                if (OtherNodeSlotDependents(dependencyGraph[slot.NodeId], searchNode.Id) == searchSlot)
                    return slot;

            return new NodeSlot(-1, -1);
        }

        /// <summary>
        /// Returns the nodeSlot that is linked to the search node's given slot in its dependencies
        /// </summary>
        /// <param name="searchNode">node to look through</param>
        /// <param name="dependencyGraph">nodes in graph</param>
        /// <param name="searchSlot">slot that other node links to</param>
        /// <returns>node slots that points to the node that is linked to the search nodes search slot</returns>
        public static NodeSlot[] FindAllNodeSlotsInDependents(DependentNode searchNode, Dictionary<int, DependentNode> dependencyGraph, int searchSlot)
        {
            List<NodeSlot> slots = new List<NodeSlot>();

            foreach (NodeSlot slot in searchNode.Dependents)
                if (OtherNodeSlotDependencies(dependencyGraph[slot.NodeId], searchNode.Id) == searchSlot)
                    slots.Add(slot);

            return slots.ToArray();
        }
    }
}
