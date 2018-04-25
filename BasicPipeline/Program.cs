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

        static void Main(string[] args)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            IEnumerable<List<byte[]>> data = new ImgInput().RetrieveData("E:\\in3");
            Parallel.ForEach(data, Process);

            timer.Stop();
            Console.WriteLine("complete " + timer.Elapsed.ToString("g"));
            Console.ReadLine();
        }

        private static void Process(List<byte[]> data)
        {
            List<byte[]> list = blur.ProcessData(data);
            jpg.ExportData("E:\\out", list);
        }
    }
}
