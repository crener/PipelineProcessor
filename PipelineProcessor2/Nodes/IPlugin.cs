using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineProcessor2.Nodes
{
    public interface  IPlugin
    {
        int InputQty { get; }
        int OutputQty { get; }

        string PluginInformation(PluginInformationRequests request, int index = 0);
    }

    public enum PluginInformationRequests
    {
        Name = 0,
        InputQty = 1,
        OutputQty = 2,
        Description = 3,
        InputName = 4,
        InputType = 4,
        OutputName = 4,
        OutputType = 4,
    }
}
