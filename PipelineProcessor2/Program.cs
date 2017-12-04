using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PipelineProcessor2.Server;

namespace PipelineProcessor2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Listener");
            AsyncServer.StartListening();

            while (AsyncServer.IsListening) Thread.Sleep(1000);
        }
    }
}
