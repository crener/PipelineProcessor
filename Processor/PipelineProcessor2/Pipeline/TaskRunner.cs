﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipelineProcessor2.Nodes;
using PipelineProcessor2.Pipeline.Exceptions;
using PluginTypes;

namespace PipelineProcessor2.Pipeline
{
    public class TaskRunner
    {
        private int run;
        private IPlugin plugin;
        private DependentNode node;
        private DataStore resultData, staticData;
        private PipelineExecutor executor;

        public TaskRunner(IPlugin plugin, DependentNode node, DataStore resultData, DataStore staticData, PipelineExecutor pipe, int run)
        {
            this.plugin = plugin;
            this.node = node;
            this.resultData = resultData;
            this.staticData = staticData;
            executor = pipe;
            this.run = run;
        }

        /// <summary>
        /// Creates the task for the plugin
        /// </summary>
        /// <returns>plugin task</returns>
        public Task getTask()
        {
            if (!ExecutionHelper.HasFulfilledDependency(node, resultData, staticData))
                return null;

            return new Task(Execute, run);
        }

        /// <summary>
        /// Executes a given nodes runtime code
        /// </summary>
        private void Execute(object ignore)
        {
            List<byte[]> data = null;
            List<byte[]> input = GatherInputData();

            Console.WriteLine(node.Type + " Starting, slot: " + node.Id + " of run " + run);

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
                    string dir = string.IsNullOrWhiteSpace(node.Value) ? PipelineState.OutputDirectory : node.Value;

                    bool success = (plugin as IOutputPlugin).ExportData(dir, input);
                    if (!success) Console.WriteLine(plugin.Name + " failed");
                }
                else Console.WriteLine("TaskRunner encountered unexpected plugin type");

                stopwatch.Stop();
                Console.WriteLine(node.Type + " Finished in " + stopwatch.Elapsed + " ms, slot: " + node.Id + " of run " + run);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new Exception("Task runner failed to execute node", e);
            }

            //post processing actions (triggering dependency, storing results)
            if (data != null) resultData.StoreResults(data, node.Id);
            executor.TriggerDependencies(node.Id);
        }

        /// <summary>
        /// Fetches input data for a node
        /// </summary>
        /// <returns>input data</returns>
        private List<byte[]> GatherInputData()
        {
            List<byte[]> input = new List<byte[]>();

            foreach (NodeSlot id in node.Dependencies)
            {
                byte[] dependencyData = resultData.getData(id);

                if (dependencyData == null) dependencyData = staticData.getData(id);
                if (dependencyData == null)
                {
                    List<byte[]> syncData = staticData.getSyncData(id);
                    if (syncData != null)
                    {
                        if (syncData.Count == 1 && !plugin.InputType(id.SlotPos).Contains("[]"))
                        {
                            //adapt sync data to a node with no expectation of array data
                            input.Add(syncData[0]);
                            continue;
                        }

                        //arrange sync node data into an array for input
                        input.Add(BitConverter.GetBytes(syncData.Count));

                        foreach (byte[] bytes in syncData)
                            input.Add(bytes);

                        continue;
                    }
                }

                if (dependencyData == null)
                    throw new MissingPluginDataException("Missing data from id: " + id.NodeId + ", slot: " +
                                                         id.SlotPos + " for node " + node.Id + " run: " + run);

                input.Add(dependencyData);
            }

            return input;
        }
    }
}
