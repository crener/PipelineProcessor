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
            jpg = new ExportJpg();
            blur = new ImageBlur();

            Stopwatch timer = new Stopwatch();
            timer.Start();
            
            //Parallel.ForEach(new ImgInput().RetrieveData("E:\\in3"), 
            //    (input) => { jpg.ExportData("E:\\out", blur.ProcessData(input)); });

            foreach(List<byte[]> input in new ImgInput().RetrieveData("E:\\in3"))
                jpg.ExportData("E:\\out", blur.ProcessData(input));

            timer.Stop();
            Console.WriteLine("complete " + timer.Elapsed.ToString("g"));
            Console.ReadLine();
        }
    }
}
