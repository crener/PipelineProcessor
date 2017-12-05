using System.Collections;
using System.Collections.Generic;

namespace PipelineProcessor2.Nodes
{
    interface IOutputPlugin : IPlugin
    {
        bool ExportData(string path, List<byte[]> saveData);
    }
}
