using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;

namespace PipelineProcessor2.Nodes.Sample
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
            return "";
        }
        public string OutputName(int slot)
        {
            if (slot == 0) return "image";
            return "";
        }
        public string InputType(int slot)
        {
            if (slot == 0) return "jpg[]";
            return "";
        }
        public string InputName(int slot)
        {
            if (slot == 0) return "image";
            return "";
        }
        #endregion

        public IEnumerable<List<byte[]>> RetrieveData(string path)
        {
            if (!Directory.Exists(path)) yield break;

            foreach (string filePath in Directory.EnumerateFiles(path))
            {
                string fileName = Path.GetFileName(filePath);
                if (fileName.EndsWith(".jpg") || fileName.EndsWith(".jpeg"))
                {
                    List<byte[]> output = new List<byte[]>();
                    try
                    {
                        output.Add(File.ReadAllBytes(filePath));
                        output.Add(Encoding.UTF8.GetBytes(fileName.Split('.')[0]));
                    }
                    catch(SecurityException io)
                    {
                        Console.WriteLine(io);
                    }
                    catch(IOException io)
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
            if(!Directory.Exists(path)) return 0;

            string[] files = Directory.GetFiles(path);
            int validCount = 0;
            foreach(string file in files)
                if(file.EndsWith(".jpg")) validCount++;

            return validCount;
        }
    }
}
