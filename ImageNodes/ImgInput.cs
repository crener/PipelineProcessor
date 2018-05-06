using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;
using PluginTypes;

namespace ImageNodes
{
    public class ImgInput : IInputPlugin
    {
        #region Node settings
        public int InputQty => 0;
        public int OutputQty => 2;
        public string Name => "Image Import";
        public string Description => "Imports all images of a given path";
        public string OutputType(int slot)
        {
            if (slot == 0) return "jpg";
            if (slot == 1) return "string";
            return "";
        }
        public string OutputName(int slot)
        {
            if (slot == 0) return "Image";
            if (slot == 1) return "Image Name";
            return "";
        }
        public string InputType(int slot) { return ""; }
        public string InputName(int slot) { return ""; }
        #endregion

        public IEnumerable<List<byte[]>> RetrieveData(string path)
        {
            if (!Directory.Exists(path)) yield break;

            foreach (string filePath in Directory.EnumerateFiles(path))
            {
                string fileName = Path.GetFileName(filePath);
                if (isValid(fileName))
                {
                    List<byte[]> output = new List<byte[]>();
                    try
                    {
                        output.Add(File.ReadAllBytes(filePath));
                        output.Add(Encoding.UTF8.GetBytes(fileName.Split('.')[0]));
                    }
                    catch (SecurityException io)
                    {
                        Console.WriteLine(io);
                    }
                    catch (IOException io)
                    {
                        Console.WriteLine(io);
                    }
                    catch (UnauthorizedAccessException io)
                    {
                        Console.WriteLine(io);
                    }

                    yield return output;
                }
            }
        }

        public int InputDataQuantity(string path)
        {
            if (!Directory.Exists(path)) return 0;

            string[] files = Directory.GetFiles(path);
            int validCount = 0;
            foreach (string file in files)
                if (isValid(file)) validCount++;

            return validCount;
        }

        private bool isValid(string file)
        {
            return file.EndsWith(".jpg") || file.EndsWith(".jpeg");
        }
    }
}
