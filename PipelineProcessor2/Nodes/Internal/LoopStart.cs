using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipelineProcessor2.Pipeline;

namespace PipelineProcessor2.Nodes.Internal
{
    [InternalNode(ShowExternal = false)]
    public class LoopStart : IRawPlugin
    {
        public const string TypeName = "Special/StartLoop";

        public int InputQty => 1;
        public int OutputQty => 1;
        public string FullName => TypeName;

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

        public string PluginInformation(PluginInformationRequests request, int index)
        {
            if (request == PluginInformationRequests.Name) return "Loop Start";
            return "";
        }

        public void AddLoopPair(LoopPair pair)
        {
            if (controllingPairs.ContainsKey(pair.Depth)) return;

            controllingPairs.Add(pair.Depth, pair);
            if (pair.Depth > maxDepth) maxDepth = pair.Depth;
        }

        public void IterateDepth()
        {
            currentDepth--;

            for (int i = 0; i < controllingPairs.Count; i++)
            {
                LoopPair pair = controllingPairs[i];
                if (pair.Depth <= currentDepth) return;

                pair.Iteration = 0;
                controllingPairs[i] = pair;
            }
        }

        public void ResetCounters()
        {
            currentDepth = maxDepth;

            for (int i = 0; i < controllingPairs.Count; i++)
            {
                LoopPair pair = controllingPairs[i];
                pair.Iteration = 0;
                controllingPairs[i] = pair;
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
            {
                LoopPair pair = controllingPairs[currentDepth];
                pair.Iteration++;
                controllingPairs[currentDepth] = pair;

                Console.WriteLine("Loop iteration: " + pair.Iteration);
            }
            else ResetCounters();

            //move the data to the nodes output
            {
                List<byte[]> outputData = new List<byte[]>();
                outputData.Add(new byte[0]); //Link slot
                //outputData.Add(new byte[0]); //increment slot
                foreach (NodeSlot slot in dependencyNode.Dependencies)
                    outputData.Add(data.getData(slot));

                data.StoreResults(outputData, NodeId);
            }

            //gather ids to be triggered
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

            return ids.ToArray();
        }
    }
}
