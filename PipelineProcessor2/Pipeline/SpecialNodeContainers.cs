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
    public struct LoopPair
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
