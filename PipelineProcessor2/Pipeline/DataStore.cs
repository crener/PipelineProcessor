using System;
using System.Collections.Generic;
using System.IO;

namespace PipelineProcessor2.Pipeline
{
    /// <summary>
    /// Responsible for storing data from a pipeline executor
    /// </summary>
    public class DataStore
    {
        private Dictionary<NodeSlot, byte[]> data = new Dictionary<NodeSlot, byte[]>();
        private int depth;
        private string cacheDir;

        public DataStore(int depth, string outputDir)
        {
            this.depth = depth;
            cacheDir = outputDir + Path.DirectorySeparatorChar + ".cache" + Path.DirectorySeparatorChar + depth + Path.DirectorySeparatorChar;
        }

        public void StoreResults(List<byte[]> newData, int nodeId, bool preventCaching = false)
        {
            for (int i = 0; i < newData.Count; i++)
            {
                NodeSlot slot = new NodeSlot(nodeId, i);
                data.Add(slot, newData[i]);
            }

            if (!preventCaching) SaveCache(newData, nodeId);
        }

        private void SaveCache(List<byte[]> byteses, int node)
        {
            string rawPath = cacheDir + node + Path.DirectorySeparatorChar;
            if (!Directory.Exists(rawPath)) Directory.CreateDirectory(rawPath);

            for (int i = 0; i < byteses.Count; i++)
            {
                byte[] data = byteses[i];
                new FileStream(rawPath + i + ".bin", FileMode.Create).WriteAsync(data, 0, data.Length);
            }
        }

        public byte[] getData(NodeSlot id)
        {
            if (data.ContainsKey(id)) return data[id];

            return null;
        }
    }
}
