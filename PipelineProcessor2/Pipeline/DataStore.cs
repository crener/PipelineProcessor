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
        private Dictionary<NodeSlot, List<byte[]>> syncData = new Dictionary<NodeSlot, List<byte[]>>();
        private string cacheDir;
        private bool disableWriting = true;

        public DataStore(int depth, string outputDir)
        {
            cacheDir = outputDir + Path.DirectorySeparatorChar + ".cache" + Path.DirectorySeparatorChar + depth + Path.DirectorySeparatorChar;
        }


        public DataStore(bool staticData)
        {
            if (!staticData)
                throw new ArgumentException("A none-static data store needs an output directory, use another constructor!");

            cacheDir = "";
            disableWriting = true;
        }

        public void StoreResults(List<byte[]> newData, int nodeId, bool preventCaching = false)
        {
            for (int i = 0; i < newData.Count; i++)
            {
                NodeSlot slot = new NodeSlot(nodeId, i);

                if (data.ContainsKey(slot)) data.Remove(slot);
                data.Add(slot, newData[i]);
            }

            if (!preventCaching) SaveCache(newData, nodeId);
            ClearIrrelevantData(nodeId);
        }

        /// <summary>
        /// Removes data from memory once it is no longer needed by the pipeline
        /// </summary>
        /// <param name="nodeId">node to check</param>
        private void ClearIrrelevantData(int nodeId)
        {
            DependentNode node;
            if (!PipelineState.DependencyGraph.TryGetValue(nodeId, out node)) return;

            foreach (NodeSlot dependency in node.Dependencies)
            {
                DependentNode searchNode;
                if (!PipelineState.DependencyGraph.TryGetValue(dependency.NodeId, out searchNode)) return;
                if (!data.ContainsKey(dependency) || data[dependency] == null) continue;

                bool needed = false;
                foreach (NodeSlot searchSlot in searchNode.Dependents)
                {
                    if (searchSlot.NodeId == nodeId) continue;

                    //todo identify if node dependents have already used this data and no longer require it
                    needed = true;
                }

                if (!needed) data[dependency] = null;
            }

            GC.Collect();
        }

        public void StoreSyncResults(List<byte[]> newData, int nodeId, int slotPos)
        {
            NodeSlot slot = new NodeSlot(nodeId, slotPos);

            if (syncData.ContainsKey(slot)) syncData.Remove(slot);
            syncData.Add(slot, newData);
        }

        public void ClearResults(List<NodeSlot> ids)
        {
            foreach (NodeSlot id in ids)
                if (data.ContainsKey(id)) data.Remove(id);
        }

        private void SaveCache(List<byte[]> byteses, int node)
        {
            if (disableWriting) return;

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

        public List<byte[]> getSyncData(NodeSlot id)
        {
            if (syncData.ContainsKey(id)) return syncData[id];

            return null;
        }
    }
}
