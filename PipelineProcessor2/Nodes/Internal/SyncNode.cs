using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using PipelineProcessor2.Pipeline;
using PipelineProcessor2.Server.Responses;
using PluginTypes;
using IRawPlugin = PipelineProcessor2.Plugin.IRawPlugin;

namespace PipelineProcessor2.Nodes.Internal
{
    [InternalNode(ShowExternal = false)]
    public class SyncNode : IRawPlugin
    {
        public const string TypeName = "Special/Sync";

        #region Node settings
        public int InputQty => 1;
        public int OutputQty => 1; public string Name => "Sync Block";
        public string FullName => TypeName;
        public string Description => "Pauses execution for all pipelines and synchronizes data between them";
        public string OutputType(int slot) { return ""; }
        public string OutputName(int slot) { return ""; }
        public string InputType(int slot) { return ""; }
        public string InputName(int slot) { return ""; }
        #endregion

        public int NodeId { get; private set; }
        public PipelineExecutor[] TriggeredPipelines => pipelines;

        private readonly Dictionary<int, List<byte[]>> data = new Dictionary<int, List<byte[]>>();
        private readonly DependentNode graphNode = null;
        private readonly DataStore staticData;
        private int parallelism = 0;
        private PipelineExecutor[] pipelines;

        private object updateLock = new object();

        public SyncNode()
        {
            //needed for plugin store
        }

        public SyncNode(DependentNode nodeId, DataStore staticData)
        {
            NodeId = nodeId.Id;
            graphNode = nodeId;
            this.staticData = staticData;

            //build dictionary
            foreach (NodeSlot node in nodeId.Dependencies)
            {
                if (data.ContainsKey(node.NodeId)) continue;
                data.Add(node.NodeId, new List<byte[]>());
            }
        }

        /// <summary>
        /// Sets the pipelines used to trigger events once data is collected
        /// </summary>
        /// <param name="incomingPipeQuantity">amount of data going into this node</param>
        /// <param name="toTrigger">pipelines that will be triggered once data is done</param>
        public void StateInfo(int incomingPipeQuantity, PipelineExecutor[] toTrigger)
        {
            parallelism = incomingPipeQuantity;
            pipelines = toTrigger;
        }

        /// <summary>
        /// Triggers data to be retrieved from a pipeline for aggregation
        /// </summary>
        /// <param name="store">pipelines data store</param>
        /// <param name="triggeredBy">node who's data will be collected</param>
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
                if (parallelism == 1)
                {
                    //staticData.StoreResults(,NodeId);
                    return;
                }

                foreach (KeyValuePair<int, List<byte[]>> pair in data)
                {
                    int slot = -1;

                    foreach (NodeSlot dependentSlot in graphNode.Dependencies)
                        if (dependentSlot.NodeId == pair.Key) slot = dependentSlot.SlotPos;

                    if (slot == -1)
                        throw new InvalidDataException("Node slots have changed since initialization");

                    staticData.StoreSyncResults(pair.Value, NodeId, slot);
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

#if DEBUG
        public int ParallelismTestOnly => parallelism;
#endif
    }
}
