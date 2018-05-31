using System.Collections.Generic;

namespace PluginTypes
{
    public interface IInputPlugin : IPlugin
    {
        /// <summary>
        /// retrieves data from the specified path to populate pipelines
        /// </summary>
        /// <param name="path">location where data should be retrieved from</param>
        /// <returns>set of output data in the order the outputs are defined in</returns>
        IEnumerable<List<byte[]>> RetrieveData(string path);
        /// <summary>
        /// Gets the total amount of data a path will return
        /// </summary>
        /// <param name="path">location were data should be retrieved from</param>
        /// <returns>amount of data that can be gathered from an endpoint</returns>
        int InputDataQuantity(string path);
    }
}
