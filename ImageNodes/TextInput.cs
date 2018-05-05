using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;
using PluginTypes;

namespace ImageNodes
{
    public class TextInput : IInputPlugin
    {
        #region Node settings
        public int InputQty => 0;
        public int OutputQty => 1;
        public string Name => "Text Import";
        public string Description => "Imports Text line by line from a text file";
        public string OutputType(int slot)
        {
            if (slot == 0) return "string";
            return "";
        }
        public string OutputName(int slot)
        {
            if (slot == 0) return "text";
            return "";
        }
        public string InputType(int slot)
        {
            return "";
        }
        public string InputName(int slot)
        {
            return "";
        }
        #endregion

        public IEnumerable<List<byte[]>> RetrieveData(string path)
        {
            if (!Directory.Exists(path)) yield break;

            using (StreamReader text = new StreamReader(new FileStream(path, FileMode.Open)))
            {
                string line;
                while ((line = text.ReadLine()) != null)
                {
                    List<byte[]> list = new List<byte[]>();
                    list.Add(Encoding.ASCII.GetBytes(line));
                    yield return list;
                }
            }
        }

        public int InputDataQuantity(string path)
        {
            if (!Directory.Exists(path)) return 0;
            if (isValid(path)) return 0;

            int count = 0;
            using (StreamReader text = new StreamReader(new FileStream(path, FileMode.Open)))
                while (text.ReadLine() != null) count++;

            return count;
        }

        private bool isValid(string file)
        {
            return file.EndsWith(".txt");
        }
    }
}
