using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipelineProcessor2.Nodes.Internal;

namespace PipelineProcessor2.Pipeline
{
    /// <summary>
    /// Structure for holding all special nodes found in the dependency graph
    /// </summary>
#if DEBUG
    public struct LoopPair
#else
    private struct LoopPair
#endif
    {
        public LoopStart Start;
        public LoopEnd End;
        public int Iteration, Id, Depth;
    }

    public struct SpecialNodeData
    {
        public List<LoopPair> Loops;
    }
}
