using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineProcessor2.Pipeline
{
    internal static class ExecutionHelper
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
    }
}
