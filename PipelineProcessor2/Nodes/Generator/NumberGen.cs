using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineProcessor2.Nodes.BasicMaths
{
    public class NumberGen : IGeneratorPlugin
    {
        public int InputQty => 0;
        public int OutputQty => 1;
        public string PluginInformation(PluginInformationRequests request, int index = 0)
        {
            if (request == PluginInformationRequests.Name) return "Number 8";
            else if (request == PluginInformationRequests.Description) return "Number 8";
            else if (request == PluginInformationRequests.OutputName)
            {
                if (index == 0) return "number";
            }
            else if (request == PluginInformationRequests.OutputType)
            {
                if (index == 0) return "int";
            }

            return "";
        }

        public List<byte[]> StaticData()
        {
            List<byte[]> output = new List<byte[]>();
            output.Add(BitConverter.GetBytes(8));

            return output;
        }
    }
}
