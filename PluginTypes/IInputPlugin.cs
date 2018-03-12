using System.Collections.Generic;

namespace PluginTypes
{
    public interface IInputPlugin : IPlugin
    {
        IEnumerable<List<byte[]>> RetrieveData(string path);
        int InputDataQuantity(string path);
    }
}
