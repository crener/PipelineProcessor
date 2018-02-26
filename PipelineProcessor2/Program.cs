using System;
using System.Threading;
using PipelineProcessor2.Plugin;
using PipelineProcessor2.Server;

namespace PipelineProcessor2
{
    class Program
    {
        static void Main(string[] args)
        {
            PluginStore.Init();

            Console.WriteLine("Starting Listener");
            AsyncServer.StartListening();
            while (AsyncServer.IsListening) Thread.Sleep(1000);
        }
    }
}
