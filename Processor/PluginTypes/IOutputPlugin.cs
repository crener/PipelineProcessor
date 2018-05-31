using System.Collections.Generic;
using PluginTypes;

namespace PluginTypes
{
    public interface IOutputPlugin : IPlugin
    {
        /// <summary>
        /// Method called when data should be written to permanent storage, Implementation must be thread-safe
        /// </summary>
        /// <param name="path">path for data to be written too</param>
        /// <param name="saveData">data to be written ordered in input slot definition</param>
        /// <returns>true if operation was successful</returns>
        bool ExportData(string path, List<byte[]> saveData);
    }
}
