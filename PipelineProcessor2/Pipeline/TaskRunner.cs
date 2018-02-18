using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipelineProcessor2.Nodes;
using PipelineProcessor2.Pipeline.Exceptions;

namespace PipelineProcessor2.Pipeline
{
    public class TaskRunner
    {
        private int run;
        private IPlugin plugin;
        private DependentNode link;
        private DataStore resultData, staticData;
        private PipelineExecutor executor;

        public TaskRunner(IPlugin plugin, DependentNode node, DataStore resultData, DataStore staticData, PipelineExecutor pipe, int run)
        {
            this.plugin = plugin;
            link = node;
            this.resultData = resultData;
            this.staticData = staticData;
            executor = pipe;
            this.run = run;
        }

        //start the plugin task
        public Task getTask()
        {
            if (!ExecutionHelper.HasFulfilledDependency(link, resultData, staticData))
                return null;

            return new Task(Execute);
        }

        private void Execute()
        {
            List<byte[]> data = null;
            List<byte[]> input = new List<byte[]>();

            Console.WriteLine(link.Type + " Starting, slot: " + link.Id + " of run " + run);

            //gather input data
            foreach (NodeSlot id in link.Dependencies)
            {
                byte[] dependencyData = resultData.getData(id);

                if(dependencyData == null) dependencyData = staticData.getData(id);

                if (dependencyData == null)
                    throw new MissingPluginDataException("Missing data from id: " + id.NodeId + ", slot: " +
                                                         id.SlotPos + " for node " + link.Id + " run: " + run);

                input.Add(dependencyData);
            }

            // execute plugin task
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

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

                stopwatch.Stop();
                Console.WriteLine(link.Type + " Finished in " + stopwatch.Elapsed +" ms, slot: " + link.Id + " of run " + run);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            //post processing actions (triggering dependency, storing results)
            if (data != null) resultData.StoreResults(data, link.Id);
            executor.TriggerDependencies(link.Id);
        }

    }
}
