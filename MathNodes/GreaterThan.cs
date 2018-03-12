using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PluginTypes;

namespace PipelineProcessor2.Nodes.BasicMaths
{
    public class GreaterThan : IProcessPlugin
    {
        #region Node settings
        public int InputQty => 2;
        public int OutputQty => 1;
        public string Name => "Greater Than";
        public string Description => "Is greater than";
        public string OutputType(int index)
        {
            if (index == 0) return "bool";
            return "";
        }
        public string OutputName(int index)
        {
            if (index == 0) return "Result";
            return "";
        }
        public string InputType(int index)
        {
            if (index == 0) return "int";
            if (index == 1) return "int";
            return "";
        }
        public string InputName(int index)
        {
            if (index == 0) return "Value";
            if (index == 1) return "Check";
            return "";
        }
        #endregion

        public List<byte[]> ProcessData(List<byte[]> input)
        {
            int val = BitConverter.ToInt32(input[0], 0),
            compair = BitConverter.ToInt32(input[1], 0);

            List<byte[]> output = new List<byte[]>();
            output.Add(new byte[] { Convert.ToByte(val > compair) });

            return output;
        }
    }
}
