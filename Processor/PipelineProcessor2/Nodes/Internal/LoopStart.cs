using System;
using System.Collections.Generic;
using System.Linq;
using PipelineProcessor2.Pipeline;
using PluginTypes;
using IRawPlugin = PipelineProcessor2.Plugin.IRawPlugin;

namespace PipelineProcessor2.Nodes.Internal
{
    [InternalNode(ShowExternal = false)]
    public class LoopStart : IRawPlugin
    {
        public const string TypeName = "Special/StartLoop";

        #region Node settings
        public int InputQty => 1;
        public int OutputQty => 1;
        public string Name => "Loop Start";
        public string FullName => TypeName;
        public string Description => "Start of a loop";
        public string OutputType(int slot) { return ""; }
        public string OutputName(int slot) { return ""; }
        public string InputType(int slot) { return ""; }
        public string InputName(int slot) { return ""; }
        #endregion

        public int NodeId { get; }

        private Dictionary<int, LoopPair> controllingPairs = new Dictionary<int, LoopPair>();
        private int currentDepth = 0, maxDepth = 0;
        private DependentNode dependencyNode;

        public LoopStart()
        {
            NodeId = -1;
        }

        public LoopStart(DependentNode graphNode)
        {
            dependencyNode = graphNode;
            NodeId = graphNode.Id;
        }

        public void AddLoopPair(ref LoopPair pair)
        {
            if (controllingPairs.ContainsKey(pair.Depth)) return;

            controllingPairs.Add(pair.Depth, pair);
            if (pair.Depth > maxDepth) maxDepth = pair.Depth;
        }

        public void ResetCounters()
        {
            currentDepth = maxDepth;

            int[] keys = controllingPairs.Keys.ToArray();
            for (int i = 0; i < keys.Length; i++)
            {
                LoopPair pair = controllingPairs[keys[i]];
                pair.Iteration = 0;
                controllingPairs[keys[i]] = pair;
            }
        }

        public int[] StartDependencies(int triggeredBy, DataStore data)
        {
            //check if it was triggered by a loop end
            bool triggeredByEnd = false;
            foreach (LoopPair pair in controllingPairs.Values)
            {
                if (triggeredBy == pair.End.NodeId)
                {
                    triggeredByEnd = true;
                    break;
                }
            }

            //iterate counters
            if (triggeredByEnd)
                Console.WriteLine("Loop iteration: " + controllingPairs[currentDepth].Iteration);
            else ResetCounters();

            //move the data to the nodes output
            if (!triggeredByEnd)
            { // end node will have moved the data already
                List<byte[]> outputData = new List<byte[]>();
                outputData.Add(new byte[0]); //Link slot
                outputData.Add(BitConverter.GetBytes(controllingPairs[currentDepth].Iteration)); //increment slot
                foreach (NodeSlot slot in dependencyNode.Dependencies)
                    outputData.Add(data.getData(slot));

                data.StoreResults(outputData, NodeId);
            }

            return getTriggerNodes().ToArray();
        }

        private List<int> getTriggerNodes()
        {
            List<int> ids = new List<int>();
            foreach (NodeSlot slot in dependencyNode.Dependents)
            {
                bool valid = true;
                foreach (LoopPair pairsValue in controllingPairs.Values)
                    if (slot.NodeId == pairsValue.End.NodeId)
                    {
                        valid = false;
                        break;
                    }

                if (!valid) continue;

                if (!ids.Contains(slot.NodeId))
                    ids.Add(slot.NodeId);
            }

            return ids;
        }
    }
}
