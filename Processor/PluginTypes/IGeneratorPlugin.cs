using System.Collections.Generic;

namespace PluginTypes
{
    public interface IGeneratorPlugin : IPlugin
    {
        /// <summary>
        /// Converts the nodeValue to static data used by all pipeline instances
        /// </summary>
        /// <param name="nodeValue">the value set in the user interface for this node to interpret</param>
        /// <returns>set of output data in the order the outputs are defined in</returns>
        List<byte[]> StaticData(string nodeValue);
        /// <summary>
        /// gets the default value to use in the user interface
        /// </summary>
        string DefaultValue { get; }
    }
}
