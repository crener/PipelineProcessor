using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageNodes;

namespace ComplexPipeline
{
    class Program
    {
        private const string outDir = "D:\\out";
        private const string inDir = "E:\\in3";
        private const int loopDuration = 3;

        static ImageBlur blur = new ImageBlur();

        static private ConcurrentBag<List<byte[]>> sync = new ConcurrentBag<List<byte[]>>();

        static void Main(string[] args)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            //Parallel.ForEach(new ImgInput().RetrieveData(inDir), Process);
            foreach(List<byte[]> data in new ImgInput().RetrieveData(inDir)) Process(data);

            {
                List<byte[]> syncData = new List<byte[]>();
                syncData.Add(BitConverter.GetBytes(sync.Count));
                foreach(List<byte[]> byteses in sync) syncData.Add(byteses[0]);

                new ExportJpg().ExportData(outDir, new BlendArray().ProcessData(syncData));
            }

            timer.Stop();
            Console.WriteLine("complete " + Math.Floor(timer.Elapsed.TotalSeconds) + "." + timer.Elapsed.Milliseconds);
            Console.ReadLine();
        }

        private static void Process(List<byte[]> data)
        {
            for(int i = 0; i < loopDuration; i++)
                data = blur.ProcessData(data);

            sync.Add(data);
        }
    }
}
