using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PluginTypes;

namespace ImageNodes
{
    public class ExportJpgName : IOutputPlugin
    {
        #region Node settings
        public int InputQty => 2;
        public int OutputQty => 0;
        public string Name => "Jpg Export Name";
        public string Description => "Saves image data to disk";
        public string OutputType(int slot) { return ""; }
        public string OutputName(int slot) { return ""; }
        public string InputType(int slot)
        {
            if (slot == 0) return "jpg";
            if (slot == 1) return "string";
            return "";
        }
        public string InputName(int slot)
        {
            if (slot == 0) return "Jpg Image";
            if (slot == 1) return "File Name";
            return "";
        }
        #endregion

        public bool ExportData(string path, List<byte[]> saveData)
        {
            string fileName = Encoding.ASCII.GetString(saveData[1]);

            try
            {
                path = path + (path.EndsWith(Path.DirectorySeparatorChar.ToString()) ? "" : Path.DirectorySeparatorChar.ToString());
                string file = path + fileName + ".jpg";

                using (var stream = new FileStream(file, FileMode.Create))
                    stream.Write(saveData[0], 0, saveData[0].Length);
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }
    }
}
