using System.Collections.Generic;

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
    }

    /// <summary>
    /// Responsible for storing data from a pipeline executor
    /// </summary>
    public class DataStore
    {
        private Dictionary<NodeSlot, byte[]> data = new Dictionary<NodeSlot, byte[]>();

        public void StoreResults(List<byte[]> newData, int nodeId)
        {
            for (int i = 0; i < newData.Count; i++)
            {
                NodeSlot slot = new NodeSlot(nodeId, i);
                data.Add(slot, newData[i]);
            }
        }

        public byte[] getData(NodeSlot id)
        {
            if (data.ContainsKey(id)) return data[id];

            return null;
        }
    }
}
