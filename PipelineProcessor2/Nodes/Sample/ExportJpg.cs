using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineProcessor2.Nodes.Sample
{
    public class ExportJpg : IOutputPlugin
    {
        public int InputQty => 1;
        public int OutputQty => 0;

        public string PluginInformation(pluginInformationRequests request, int index)
        {
            if (request == pluginInformationRequests.Name) return "Jpg Export";
            else if (request == pluginInformationRequests.description) return "Saves image data to disk";
            else if (request == pluginInformationRequests.inputName)
            {
                if (index == 0) return "Jpg Image";
            }
            else if (request == pluginInformationRequests.inputType)
            {
                if (index == 0) return "jpg";
            }

            return "";
        }

        public bool ExportData(string path, List<byte[]> saveData)
        {
            throw new NotImplementedException();
        }
    }
}
