using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineProcessor2.Nodes.Sample
{
    public class ExportJpg : IOutputPlugin
    {
        public int InputQty => 1;
        public int OutputQty => 0;

        private static Random rand = new Random();

        public string PluginInformation(PluginInformationRequests request, int index)
        {
            if (request == PluginInformationRequests.Name) return "Jpg Export";
            else if (request == PluginInformationRequests.Description) return "Saves image data to disk";
            else if (request == PluginInformationRequests.InputName)
            {
                if (index == 0) return "Jpg Image";
            }
            else if (request == PluginInformationRequests.InputType)
            {
                if (index == 0) return "jpg";
            }

            return "";
        }

        public bool ExportData(string path, List<byte[]> saveData)
        {
            try
            {
                path = path + (path.EndsWith(Path.DirectorySeparatorChar.ToString()) ? "" : Path.DirectorySeparatorChar.ToString());
                string file;
                do
                {
                    file = path + rand.Next() + ".jpg";
                } while (File.Exists(file));

                using (var stream = new FileStream(file, FileMode.Create))
                    stream.Write(saveData[0], 0, saveData[0].Length);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
