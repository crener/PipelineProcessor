using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineProcessor2.Pipeline
{
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
