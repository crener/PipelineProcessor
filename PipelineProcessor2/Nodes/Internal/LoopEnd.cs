using System;
using System.Collections.Generic;
using PipelineProcessor2.Pipeline;
using PluginTypes;
using IRawPlugin = PipelineProcessor2.Plugin.IRawPlugin;

namespace PipelineProcessor2.Nodes.Internal
{
    [InternalNode(ShowExternal = false)]
    public class LoopEnd : IRawPlugin
    {
        public const string TypeName = "Special/EndLoop";

        #region Node settings
        public int InputQty => 1;
        public int OutputQty => 1;
        public string Name => "Loop End";
        public string FullName => TypeName;
        public string Description => "End of a loop";
        public string OutputType(int slot) { return ""; }
        public string OutputName(int slot) { return ""; }
        public string InputType(int slot)
        {
            if (slot == 0) return "int";
            return "";
        }
        public string InputName(int slot)
        {
            if (slot == 0) return "Limit";
            return "";
        }
        #endregion

        public int NodeId { get; }

        private DependentNode node;
        private LoopPair pair;

        public LoopEnd()
        {
            NodeId = -1;
        }


        public LoopEnd(DependentNode node, LoopPair pair)
        {
            NodeId = node.Id;
            this.node = node;
            this.pair = pair;
        }

        /// <summary>
        /// Checks if the loop has ended
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dependencyGraph"></param>
        /// <returns>next set of Ids to be triggered</returns>
        public int[] Finished(DataStore data, Dictionary<int, DependentNode> dependencyGraph)
        {
            //check condition
            bool done = false;
            {
                //find the "done" result
                NodeSlot search = ExecutionHelper.FindFirstNodeSlotInDependencies(node, dependencyGraph, 1);
                if (search.NodeId == -1) return new int[0];

                byte[] result = data.getData(search);
                if (result == null) return new int[0];

                if (Convert.ToBoolean(result[0])) done = true;
            }

            //trigger dependencies and iterate loop start depth
            if (done)
            {
                //move the data through to the output of the node
                List<byte[]> transferData = new List<byte[]>();
                for (int j = 2; j < node.Dependencies.Length; j++)
                    for (var i = 0; i < node.Dependencies.Length; i++)
                    {
                        NodeSlot slot = node.Dependencies[i];
                        int slotPos = ExecutionHelper.OtherNodeSlotDependents(dependencyGraph[slot.NodeId], node.Id);

                        //slot 0 and 1 are loop node slots, all other data must be passed on
                        if (slotPos == j)
                        {
                            transferData.Add(data.getData(node.Dependencies[i]));
                            break;
                        }
                    }

                data.StoreResults(transferData, NodeId);

                //trigger next nodes
                List<int> ids = new List<int>();
                foreach (NodeSlot slot in node.Dependents)
                    if (!ids.Contains(slot.NodeId)) ids.Add(slot.NodeId);

                return ids.ToArray();
            }

            //move loop end input data to the loop start output
            //so that it can be used in the next cycle

            List<byte[]> outputData = new List<byte[]>();
            outputData.Add(new byte[0]); //loop start Link slot
            outputData.Add(BitConverter.GetBytes(++pair.Iteration)); //loop start increment slot

            for (int j = 2; j < node.Dependencies.Length; j++)
                for (var i = 0; i < node.Dependencies.Length; i++)
                {
                    NodeSlot slot = node.Dependencies[i];
                    int slotPos = ExecutionHelper.OtherNodeSlotDependents(dependencyGraph[slot.NodeId], node.Id);

                    //slot 0 and 1 are loop node slots, all other data must be passed on
                    if (slotPos == j)
                    {
                        outputData.Add(data.getData(node.Dependencies[i]));
                        break;
                    }
                }

            data.ClearResults(pair.ContainedNodes);
            data.StoreResults(outputData, pair.Start.NodeId);

            return pair.Start.StartDependencies(NodeId, data);
        }
    }
}
