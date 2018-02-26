using System;
using System.Collections.Generic;
using PipelineProcessor2.Nodes.Internal;

namespace PipelineProcessor2.Pipeline
{
    /// <summary>
    /// Structure for holding all special nodes found in the dependency graph
    /// </summary>
    public class LoopPair
    {
        public LoopStart Start;
        public LoopEnd End;
        public int Iteration, Id, Depth;
        public List<NodeSlot> ContainedNodes;
    }

    public struct SpecialNodeData
    {
        public List<LoopPair> Loops;
    }
}
