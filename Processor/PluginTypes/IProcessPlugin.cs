using System.Collections.Generic;

namespace PluginTypes
{
    public interface IProcessPlugin : IPlugin
    {
        /// <summary>
        /// An operation on data with takes a nodes input connection and outputs the result through the nodes outputs, Implementation must be thread-safe
        /// 
        /// Note: if a node specifies an array input the normal index location is replaced with a number indicating how many of the following input data items are related
        /// to the array.
        /// </summary>
        /// <param name="input">data from a nodes inputs ordered as specified by input information</param>
        /// <returns>result of operation in the same order as the nodes output definition</returns>
        List<byte[]> ProcessData(List<byte[]> input);
    }
}
