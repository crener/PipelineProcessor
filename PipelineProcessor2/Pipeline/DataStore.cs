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
        private bool disableWriting = true;

        public DataStore(int depth, string outputDir)
        {
            this.depth = depth;
            cacheDir = outputDir + Path.DirectorySeparatorChar + ".cache" + Path.DirectorySeparatorChar + depth + Path.DirectorySeparatorChar;
        }
        public DataStore(bool staticData)
        {
            if(!staticData)
                throw new ArgumentException("A none-static data store needs an output directory, use another constructor!");

            depth = -1;
            cacheDir = "";
            disableWriting = true;
        }

        public void StoreResults(List<byte[]> newData, int nodeId, bool preventCaching = false)
        {
            for (int i = 0; i < newData.Count; i++)
            {
                NodeSlot slot = new NodeSlot(nodeId, i);

                if(data.ContainsKey(slot)) data.Remove(slot);
                data.Add(slot, newData[i]);
            }

            if (!preventCaching) SaveCache(newData, nodeId);
        }

        private void SaveCache(List<byte[]> byteses, int node)
        {
            if(disableWriting) return;

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
