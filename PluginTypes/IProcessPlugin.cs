using System.Collections.Generic;

namespace PluginTypes
{
    public interface IProcessPlugin : IPlugin
    {
        /// <summary>
        /// An operation on data with takes a nodes input connection and outputs the result through the nodes outputs, Implementation must be thread-safe
        /// </summary>
        /// <param name="input">data from a nodes inputs ordered as specified by input information</param>
        /// <returns>result of operation in the same order as the nodes output definition</returns>
        List<byte[]> ProcessData(List<byte[]> input);
    }
}
