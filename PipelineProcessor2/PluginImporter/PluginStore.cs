using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using PipelineProcessor2.Nodes;
using PipelineProcessor2.Server;

namespace PipelineProcessor2.PluginImporter
{
    public static class PluginStore
    {
        static Dictionary<string, IPlugin> plugins = new Dictionary<string, IPlugin>();
        static List<Node> nodes = new List<Node>();

        private static object pluginLock = new object(),
            nodeLock = new object();

        public static void Init()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    try
                    {
                        if (typeof(IPlugin).IsAssignableFrom(type) && !type.IsInterface)
                        {
                            IPlugin plugin = (IPlugin)Activator.CreateInstance(type);
                            Console.WriteLine("Adding Plugin: " + plugin.PluginInformation(PluginInformationRequests.Name, 0));
                            AddPlugin(plugin);
                        }
                    }
                    catch (InvalidCastException) { } //ignore
                }
            }
        }

        public static void AddPlugin(IPlugin plugin)
        {
            Node nodeData = new Node(
                plugin.PluginInformation(PluginInformationRequests.Name, 0),
                plugin.PluginInformation(PluginInformationRequests.Description, 0),
                "C#");

            if (plugin is IInputPlugin) nodeData.category = "Input";
            else if (plugin is IOutputPlugin) nodeData.category = "Output";
            else if (plugin is IProcessPlugin) nodeData.category = "Process";

            lock (pluginLock)
            {
                if (plugins.ContainsKey(nodeData.getTypeVal()))
                {
                    Console.WriteLine("Plugin duplicate detected!! " + nodeData.title);
                    return;
                }
            }

            for (int i = 0; i < plugin.InputQty; i++)
            {
                NodeInputOutput inOut = new NodeInputOutput();
                inOut.name = plugin.PluginInformation(PluginInformationRequests.InputName, i);
                inOut.type = plugin.PluginInformation(PluginInformationRequests.InputType, i);

                nodeData.input.Add(inOut);
            }

            for (int i = 0; i < plugin.OutputQty; i++)
            {
                NodeInputOutput inOut = new NodeInputOutput();
                inOut.name = plugin.PluginInformation(PluginInformationRequests.OutputName, i);
                inOut.type = plugin.PluginInformation(PluginInformationRequests.OutputType, i);

                nodeData.output.Add(inOut);
            }

            lock (nodeLock) nodes.Add(nodeData);
            lock (pluginLock) plugins.Add(nodeData.getTypeVal(), plugin);
        }

        public static Node[] AvailableNodes()
        {
            lock (nodeLock)
            {
                return nodes.ToArray();
            }
        }
    }
}
