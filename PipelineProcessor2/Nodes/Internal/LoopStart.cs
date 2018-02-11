using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public LoopStart()
        {
            NodeId = -1;
        }

        public LoopStart(int nodeId)
        {
            this.NodeId = nodeId;
        }

        public string PluginInformation(PluginInformationRequests request, int index)
        {
            if (request == PluginInformationRequests.Name) return "Loop Start";
            return "";
        }

    }
}
