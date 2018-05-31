using System;
using System.Collections.Generic;
using System.Text;
using PluginTypes;

namespace PipelineProcessor2.Nodes.Generator
{
    public class StringGen : IGeneratorPlugin
    {
        #region Node settings
        public int InputQty => 0;
        public int OutputQty => 1;
        public string DefaultValue => "";
        public string Name => "Text";
        public string Description => "Text";
        public string OutputType(int slot)
        {
            if (slot == 0) return "string";
            return "";
        }
        public string OutputName(int slot)
        {
            if (slot == 0) return "Text";
            return "";
        }
        public string InputType(int slot) { return ""; }
        public string InputName(int slot) { return ""; }
        #endregion

        public List<byte[]> StaticData(string nodeValue)
        {
            List<byte[]> output = new List<byte[]>();
            output.Add(Encoding.ASCII.GetBytes(nodeValue));

            return output;
        }
    }
}
