using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineProcessor2.Nodes.Internal
{
    interface IRawPlugin : IPlugin
    {
        string FullName { get; }
    }
}
