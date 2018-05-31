using System;
using System.Collections.Generic;
using PluginTypes;

namespace PipelineProcessor2.Nodes.Generator
{
    public class NumberGen : IGeneratorPlugin
    {
        #region Node settings
        public int InputQty => 0;
        public int OutputQty => 1;
        public string DefaultValue => "8";
        public string Name => "Number";
        public string Description => "Number";
        public string OutputType(int slot)
        {
            if (slot == 0) return "int";
            return "";
        }
        public string OutputName(int slot)
        {
            if (slot == 0) return "number";
            return "";
        }
        public string InputType(int slot) { return ""; }
        public string InputName(int slot) { return ""; }
        #endregion

        public List<byte[]> StaticData(string nodeValue)
        {
            List<byte[]> output = new List<byte[]>();

            int value = int.Parse(nodeValue);
            output.Add(BitConverter.GetBytes(value));

            return output;
        }
    }
}
