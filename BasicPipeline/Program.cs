using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageNodes;

namespace BasicPipeline
{
    class Program
    {
        static ExportJpg jpg = new ExportJpg();
        static ImageBlur blur = new ImageBlur();

        private const string outDir = "D:\\out";
        private const string inDir = "E:\\in3";

        static void Main(string[] args)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            ImgInput inputNode = new ImgInput();
            List<List<byte[]>> allData = new List<List<byte[]>>(inputNode.RetrieveData(inDir));

            //Parallel.ForEach(allData,
            //    (input) => { jpg.ExportData(outDir, blur.ProcessData(input)); });

            foreach (List<byte[]> input in allData)
                jpg.ExportData(outDir, blur.ProcessData(input));

            timer.Stop();
            Console.WriteLine("complete " + timer.Elapsed.ToString("g"));
            Console.ReadLine();
        }
    }
}
