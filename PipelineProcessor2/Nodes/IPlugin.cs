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
        string Description { get; }
        string Name { get; }

        string OutputType(int slot);
        string OutputName(int slot);
        string InputType(int slot);
        string InputName(int slot);
    }

    public enum PluginInformationRequests
    {
        Name = 0,
        Description = 3,
        InputName = 4,
        InputType = 4,
        OutputName = 4,
        OutputType = 4,
    }
}
