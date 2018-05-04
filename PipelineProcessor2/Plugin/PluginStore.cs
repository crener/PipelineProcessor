using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using PipelineProcessor2.JsonTypes;
using PipelineProcessor2.Nodes;
using PipelineProcessor2.Nodes.Internal;
using PluginTypes;

namespace PipelineProcessor2.Plugin
{
    public static class PluginStore
    {
        static Dictionary<string, IPlugin> plugins = new Dictionary<string, IPlugin>();
        static Dictionary<string, IInputPlugin> input = new Dictionary<string, IInputPlugin>();
        static List<string> generator = new List<string>(),
            internalPlugins = new List<string>(),
            export = new List<string>(),
            processor = new List<string>();
        static List<Node> nodes = new List<Node>();

        public static void Init()
        {
            // get all available plugins defined within the assembly
            Console.WriteLine("Searching for internal Plugins");
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                LoadPluginsFromAssembly(assembly);
            }

            //load external assemblies
            Console.WriteLine("\nSearching for external Plugin libraries");
            string localPath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "Plugin";
            if (Directory.Exists(localPath))
            {
                foreach (string path in Directory.GetFiles(localPath, "*.dll", SearchOption.AllDirectories))
                {
                    try
                    {
                        Assembly file = Assembly.LoadFile(path);
                        Console.WriteLine("\n-Loading from " + Path.GetFileName(path));
                        LoadPluginsFromAssembly(file);
                    }
                    catch (Exception) { }
                }

                Console.WriteLine("\nPlugin loading complete\n---------------");
            }
        }

        private static void LoadPluginsFromAssembly(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                try
                {
                    if (type.IsInterface || !typeof(IPlugin).IsAssignableFrom(type)) continue;

                    IPlugin plugin = Activator.CreateInstance(type) as IPlugin;
                    if (plugin == null) continue;

                    Attribute internalAttribute = type.GetCustomAttribute(typeof(InternalNode));
                    if (internalAttribute != null && !((InternalNode)internalAttribute).ShowExternal)
                    {
                        AddInternal(plugin);
                        continue;
                    }

                    Console.WriteLine("Adding Plugin: " + plugin.Name, 0);
                    AddPlugin(plugin);
                }
                catch (InvalidCastException) { } //ignore
                catch (MissingMethodException mme)
                {
                    Console.WriteLine("Failed Adding: " + type.Name + ", " + mme.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed Adding: " + type.Name + ", " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Adds a new node to the node database
        /// </summary>
        /// <param name="plugin">Node to be added</param>
        public static void AddPlugin(IPlugin plugin)
        {
            Node nodeData = new Node(plugin.Name, plugin.Description, "C#");

            if (plugin is IInputPlugin) nodeData.Category = "Input";
            else if (plugin is IOutputPlugin) nodeData.Category = "Output";
            else if (plugin is IProcessPlugin) nodeData.Category = "Process";
            else if (plugin is IGeneratorPlugin) nodeData.Category = "Generator";

            lock (plugins)
            {
                if (plugins.ContainsKey(nodeData.getTypeVal()))
                {
                    Console.WriteLine("Plugin duplicate detected!! " + nodeData.Title);
                    return;
                }
            }

            for (int i = 0; i < plugin.InputQty; i++)
            {
                NodeInputOutput inOut = new NodeInputOutput();
                inOut.name = plugin.InputName(i);
                inOut.type = plugin.InputType(i);

                nodeData.Input.Add(inOut);
            }

            for (int i = 0; i < plugin.OutputQty; i++)
            {
                NodeInputOutput inOut = new NodeInputOutput();
                inOut.name = plugin.OutputName(i);
                inOut.type = plugin.OutputType(i);

                nodeData.Output.Add(inOut);
            }

            string type = nodeData.getTypeVal();

            if (plugin is IInputPlugin)
            {
                lock (input) input.Add(type, plugin as IInputPlugin);

                nodeData.UseValue = true;
                nodeData.DefaultValue = "";
            }

            if (plugin is IProcessPlugin)
                lock (processor) processor.Add(type);

            if (plugin is IOutputPlugin)
            {
                lock (export) export.Add(type);

                nodeData.UseValue = true;
                nodeData.DefaultValue = "";
            }

            if (plugin is IGeneratorPlugin)
            {
                lock (generator) generator.Add(type);

                nodeData.UseValue = true;
                nodeData.DefaultValue = (plugin as IGeneratorPlugin).DefaultValue;
            }

            lock (nodes) nodes.Add(nodeData);
            lock (plugins) plugins.Add(type, plugin);
        }

        public static void AddInternal(IPlugin plugin)
        {
            string name = "";

            if (plugin is IRawPlugin) name = (plugin as IRawPlugin).FullName;
            else name = "internal/" + plugin.Name;

            internalPlugins.Add(name);
        }

        public static Node[] AvailableNodes()
        {
            lock (nodes)
            {
                return nodes.ToArray();
            }
        }

        public static IInputPlugin getInputPlugin(string pluginName)
        {
            lock (input)
            {
                if (!input.ContainsKey(pluginName)) return null;
                return input[pluginName];
            }
        }

        public static IPlugin getPlugin(string pluginName)
        {
            lock (plugins)
            {
                if (!plugins.ContainsKey(pluginName)) return null;
                return plugins[pluginName];
            }
        }

        public static bool isRegisteredPlugin(string pluginType)
        {
#if DEBUG
            if (pluginType == "" ||
                pluginType == "in" ||
                pluginType == "end" ||
                pluginType == "pro" ||
                pluginType == "gen") return true;
#endif

            lock (plugins) if (plugins.ContainsKey(pluginType)) return true;
            if (internalPlugins.Contains(pluginType)) return true;

            return false;
        }

        public static bool isInternalPlugin(string pluginType)
        {
            return internalPlugins.Contains(pluginType);
        }

        public static bool isInputPlugin(string pluginType)
        {
#if DEBUG
            if (pluginType == "in") return true;
#endif
            lock (input)
            {
                return input.ContainsKey(pluginType);
            }
        }

        public static bool isOutputPlugin(string pluginType)
        {
#if DEBUG
            if (pluginType.StartsWith("end")) return true;
#endif
            lock (export)
            {
                return export.Contains(pluginType);
            }
        }

        public static bool isProcessingPlugin(string pluginType)
        {
#if DEBUG
            if (pluginType.StartsWith("pro")) return true;
#endif
            lock (processor)
            {
                return processor.Contains(pluginType);
            }
        }

        public static bool isGeneratorPlugin(string pluginType)
        {
#if DEBUG
            if (pluginType.StartsWith("gen")) return true;
#endif
            lock (generator)
            {
                return generator.Contains(pluginType);
            }
        }

#if DEBUG
        public static void ClearAll()
        {
            lock (generator) generator.Clear();
            lock (processor) processor.Clear();
            internalPlugins.Clear();
            lock (export) export.Clear();
            lock (nodes) nodes.Clear();

            lock (plugins) plugins.Clear();
            lock (input) input.Clear();
        }
#endif
    }
}
