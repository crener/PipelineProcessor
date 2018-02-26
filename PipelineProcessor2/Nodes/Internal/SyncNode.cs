using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using PipelineProcessor2.Pipeline;
using PipelineProcessor2.Server.Responses;

namespace PipelineProcessor2.Nodes.Internal
{
    public class SyncNode : IRawPlugin
    {
        public const string TypeName = "Special/Sync";

        public int InputQty => 1;
        public int OutputQty => 1;
        public string FullName => TypeName;
        public int NodeId { get; private set; }

        private readonly Dictionary<int, List<byte[]>> data = new Dictionary<int, List<byte[]>>();
        private readonly DependentNode graphNode = null;
        private int parallelism = 0;
        private PipelineExecutor[] pipelines;

        private object updateLock;

        private SyncNode()
        {

        }

        public SyncNode(DependentNode nodeId)
        {
            NodeId = nodeId.Id;
            graphNode = nodeId;

            //build dictionary
            foreach (NodeSlot node in nodeId.Dependencies)
                data.Add(node.NodeId, new List<byte[]>());
        }

        public void StateInfo(PipelineExecutor[] initialRuns)
        {
            parallelism = initialRuns.Length;
            pipelines = initialRuns;
        }

        public void ClearData()
        {
            lock (updateLock)
            {
                foreach (List<byte[]> list in data.Values)
                    list.Clear();
            }
        }

        public void StoreData(DataStore store, int triggeredBy)
        {
            NodeSlot slot = NodeSlot.Invalid;
            foreach (NodeSlot dependentSlot in graphNode.Dependencies)
                if (dependentSlot.NodeId == triggeredBy) slot = dependentSlot;

            if (NodeSlot.isInvalid(slot)) return;

            byte[] newData = store.getData(slot);

            lock (updateLock)
            {
                data[triggeredBy].Add(newData);
            }

            if (ReadyToTrigger())
            {
                Console.WriteLine("Sync Block done, " + (data.Count * parallelism) + " results collected");

                StoreResultData();
                TriggerPipelines();
            }
        }

        private void StoreResultData()
        {
            lock (updateLock)
            {
                foreach (KeyValuePair<int, List<byte[]>> pair in data)
                {
                    int slot = -1;

                    foreach (NodeSlot dependentSlot in graphNode.Dependencies)
                        if (dependentSlot.NodeId == pair.Key) slot = dependentSlot.SlotPos;

                    if (slot == -1)
                        throw new InvalidDataException("Node slots have changed since initialization");

                    foreach (PipelineExecutor pipe in pipelines)
                        pipe.StoreSyncData(pair.Value, NodeId, slot);
                }
            }
        }

        private bool ReadyToTrigger()
        {
            lock (updateLock)
            {
                foreach (List<byte[]> list in data.Values)
                    if (list.Count != parallelism) return false;
            }

            return true;
        }

        private void TriggerPipelines()
        {
            foreach (PipelineExecutor pipeline in pipelines)
            {
                pipeline.TriggerDependencies(NodeId);
            }
        }

        public string PluginInformation(PluginInformationRequests request, int index = 0)
        {
            throw new NotImplementedException();
        }
    }
}
