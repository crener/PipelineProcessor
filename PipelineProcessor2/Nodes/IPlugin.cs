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

        string PluginInformation(pluginInformationRequests request, int index);
    }

    public enum pluginInformationRequests
    {
        Name = 0,
        inputQty = 1,
        outputQty = 2,
        description = 3,
        inputName = 4,
        inputType = 4,
        outputName = 4,
        outputType = 4,
    };
}
