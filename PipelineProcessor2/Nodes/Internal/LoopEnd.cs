using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineProcessor2.Nodes.Internal
{
    [InternalNode(ShowExternal = false)]
    public class LoopEnd : IRawPlugin
    {
        public const string TypeName = "Special/EndLoop";

        public int InputQty => 1;
        public int OutputQty => 1;
        public string FullName => TypeName;

        public int NodeId { get; }

        public LoopEnd(int nodeId)
        {
            this.NodeId = nodeId;
        }

        public string PluginInformation(PluginInformationRequests request, int index)
        {
            if (request == PluginInformationRequests.Name) return "Loop End";
            else if (request == PluginInformationRequests.Description) return "End of a loop";
            else if (request == PluginInformationRequests.InputName)
            {
                if (index == 0) return "Limit";
            }
            else if (request == PluginInformationRequests.InputType)
            {
                if (index == 0) return "int";
            }

            return "";
        }
    }
}
