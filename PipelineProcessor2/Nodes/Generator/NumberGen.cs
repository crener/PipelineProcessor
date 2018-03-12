using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PluginTypes;

namespace PipelineProcessor2.Nodes.BasicMaths
{
    public class NumberGen : IGeneratorPlugin
    {
        #region Node settings
        public int InputQty => 0;
        public int OutputQty => 1;
        public string Name => "Number 8";
        public string Description => "Number 8";
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

        public List<byte[]> StaticData()
        {
            List<byte[]> output = new List<byte[]>();
            output.Add(BitConverter.GetBytes(8));

            return output;
        }
    }
}
