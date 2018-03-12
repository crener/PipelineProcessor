using System.Collections.Generic;
using PluginTypes;

namespace PluginTypes
{
    public interface IOutputPlugin : IPlugin
    {
        bool ExportData(string path, List<byte[]> saveData);
    }
}
