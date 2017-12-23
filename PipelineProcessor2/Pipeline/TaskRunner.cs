using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipelineProcessor2.Nodes;

namespace PipelineProcessor2.Pipeline
{
    public class TaskRunner
    {
        private IPlugin plugin;
        private DependentNode link;
        private DataStore resultData;
        private PipelineExecutor executor;

        public TaskRunner(IPlugin plugin, DependentNode node, DataStore resultData, PipelineExecutor pipe)
        {
            this.plugin = plugin;
            link = node;
            this.resultData = resultData;
            executor = pipe;
        }

        //start the plugin task
        public Task getTask()
        {
            if (!HasFulfilledDependency()) return null;

            return new Task(Execute);
        }

        private void Execute()
        {
            List<byte[]> data = null;
            List<byte[]> input = new List<byte[]>();

            foreach (NodeSlot id in link.Dependencies)
            {
                byte[] dependencyData = resultData.getData(id);
                if (dependencyData == null)
                    throw new MissingPluginDataException("Missing data from id: " + id.NodeId + ", slot: " +
                                                         id.SlotPos + " for node " + link.Id);

                input.Add(dependencyData);
            }

            try
            {
                if (plugin is IProcessPlugin)
                {
                    data = (plugin as IProcessPlugin).ProcessData(input);
                }
                else if (plugin is IOutputPlugin)
                {
                    bool success = (plugin as IOutputPlugin).ExportData(PipelineState.OutputDirectory, input);
                    if (!success) Console.WriteLine(plugin.PluginInformation(PluginInformationRequests.Name, 0) + " failed");
                }
                else Console.WriteLine("Unknown plugin type");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            //post processing actions
            if (data != null)
            {
                resultData.StoreResults(data, link.Id);
                executor.TriggerDependencies(link.Id);
            }
        }

        public bool HasFulfilledDependency()
        {
            foreach (NodeSlot id in link.Dependencies)
                if (resultData.getData(id) == null) return false;

            return true;
        }
    }
}
