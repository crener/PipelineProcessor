using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PipelineProcessor2.Nodes.Internal
{
    [InternalNode(ShowExternal = false)]
    public class LoopDurationLimit : IRawPlugin
    {
        public const string TypeName = "Special/LoopLimit";

        public int InputQty => 1;
        public int OutputQty => 1;
        public string FullName => TypeName;

        public string PluginInformation(PluginInformationRequests request, int index)
        {
            if (request == PluginInformationRequests.Name) return "Loop Duration Limit";
            else if (request == PluginInformationRequests.Description) return "Limits loops to a specific amount of iterations";
            else if (request == PluginInformationRequests.InputName)
            {
                if (index == 0) return "Limit";
            }
            else if (request == PluginInformationRequests.InputType)
            {
                if (index == 0) return "int";
            }
            else if (request == PluginInformationRequests.OutputName)
            {
                if (index == 0) return "Over";
            }
            else if (request == PluginInformationRequests.OutputType)
            {
                if (index == 0) return "bool";
            }

            return "";
        }

        private int count = 0;

        public bool isDone()
        {
            if(count >= 8) return true;
            return false;
        }
    }
}
