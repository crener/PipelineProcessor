using System;
using System.Collections.Generic;
using PipelineProcessor2.Nodes.Internal;

namespace PipelineProcessor2.Pipeline
{
    /// <summary>
    /// Structure for holding all special nodes found in the dependency graph
    /// </summary>
    public class LoopPair
    {
        public LoopStart Start;
        public LoopEnd End;
        public int Iteration, Id, Depth;
        public List<NodeSlot> ContainedNodes;
    }

    public struct SpecialNodeData
    {
        public List<LoopPair> Loops;
    }

    /// <summary>
    /// structure which indicates the connection between two nodes
    /// </summary>
    public struct NodeSlot
    {
        public int NodeId;
        public int SlotPos;

        public NodeSlot(int nodeId, int slotPos)
        {
            NodeId = nodeId;
            SlotPos = slotPos;
        }

        public static NodeSlot Invalid => new NodeSlot(-1, -1);

        public static bool isInvalid(NodeSlot check)
        {
            return check.NodeId < 0 || check.SlotPos < -1;
        }
    }
}
