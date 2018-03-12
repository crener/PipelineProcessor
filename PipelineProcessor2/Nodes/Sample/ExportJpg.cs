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
        #region Node settings
        public int InputQty => 1;
        public int OutputQty => 0;
        public string Name => "Jpg Export";
        public string Description => "Saves image data to disk";
        public string OutputType(int slot) { return ""; }
        public string OutputName(int slot) { return ""; }
        public string InputType(int slot)
        {
            if (slot == 0) return "jpg";
            return "";
        }
        public string InputName(int slot)
        {
            if (slot == 0) return "Jpg Image";
            return "";
        }
        #endregion

        private static Random rand = new Random();

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
