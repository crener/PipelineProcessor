namespace PluginTypes
{
    public interface  IPlugin
    {
        /// <summary>
        /// Amount of input connections
        /// </summary>
        int InputQty { get; }

        /// <summary>
        /// Amount of output connections
        /// </summary>
        int OutputQty { get; }

        /// <summary>
        /// Description of the node
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Node of the node
        /// </summary>
        string Name { get; }


        /// <summary>
        /// Type of data that is output
        /// </summary>
        /// <param name="slot">slot that data type information is wanted for</param>
        /// <returns>output data type for the given slot</returns>
        string OutputType(int slot);
        /// <summary>
        /// Name of the output slot
        /// </summary>
        /// <param name="slot">slot that the name is wanted for</param>
        /// <returns>output name for the given slot</returns>
        string OutputName(int slot);
        /// <summary>
        /// Type of data that is valid for an input node, multiple types can be specified in a comma separated list
        /// </summary>
        /// <param name="slot">slot that data type information is wanted for</param>
        /// <returns>input data type for the given slot</returns>
        string InputType(int slot);
        /// <summary>
        /// Name of the input slot
        /// </summary>
        /// <param name="slot">slot that the name is wanted for</param>
        /// <returns>inout name for the given slot</returns>
        string InputName(int slot);
    }
}
