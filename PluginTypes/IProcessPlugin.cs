using System.Collections.Generic;

namespace PluginTypes
{
    public interface IProcessPlugin : IPlugin
    {
        List<byte[]> ProcessData(List<byte[]> input);
    }
}
