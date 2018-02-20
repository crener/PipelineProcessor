using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineProcessor2.Nodes.BasicMaths
{
    public class GreaterThanEqual : IProcessPlugin
    {
        public int InputQty => 2;
        public int OutputQty => 1;
        public string PluginInformation(PluginInformationRequests request, int index = 0)
        {
            if (request == PluginInformationRequests.Name) return "Greater Than or Equal";
            else if (request == PluginInformationRequests.Description) return "Is greater than or Equal";
            else if (request == PluginInformationRequests.InputName)
            {
                if (index == 0) return "Comparison";
                if (index == 1) return ">=";
            }
            else if (request == PluginInformationRequests.InputType)
            {
                if (index == 0) return "int";
                if (index == 1) return "int";
            }
            else if (request == PluginInformationRequests.OutputName)
            {
                if (index == 0) return "Result";
            }
            else if (request == PluginInformationRequests.OutputType)
            {
                if (index == 0) return "bool";
            }

            return "";
        }

        public List<byte[]> ProcessData(List<byte[]> input)
        {
            int val = BitConverter.ToInt32(input[0], 0),
            compair = BitConverter.ToInt32(input[1], 0);

            List<byte[]> output = new List<byte[]>();
            output.Add(new byte[] { Convert.ToByte(val >= compair) });

            return output;
        }
    }
}
