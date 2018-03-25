using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PipelineProcessor2.Pipeline
{
    public class PipelineScheduler : TaskScheduler
    {
        [ThreadStatic]
        private static bool threadWorking;

        SortedList<int, List<Task>> tasks = new SortedList<int, List<Task>>();
        private int totalJobs = 0;


        protected override void QueueTask(Task task)
        {
            int depth = task.AsyncState is int ? (int)task.AsyncState : int.MaxValue;

            if (tasks.ContainsKey(depth)) tasks[depth].Add(task);
            else
            {
                tasks.Add(depth, new List<Task>());
                tasks[depth].Add(task);
            }

            if (totalJobs < MaximumConcurrencyLevel)
            {
                ++totalJobs;
                NotifyThreadPoolOfPendingWork();
            }
        }

        private void NotifyThreadPoolOfPendingWork()
        {
            ThreadPool.UnsafeQueueUserWorkItem(_ =>
            {
                threadWorking = true;
                try
                {
                    // Process all available items in the queue.
                    while (true)
                    {
                        Task item;
                        lock (tasks)
                        {
                            if (tasks.Count <= 0)
                            {
                                totalJobs = 0;
                                break;
                            }

                            // Get the next item from the queue
                            List<Task> list = tasks[tasks.Keys[0]];
                            item = list[0];

                            list.RemoveAt(0);
                            if (list.Count == 0) tasks.RemoveAt(0);
                        }

                        TryExecuteTask(item);
                        --totalJobs;
                    }
                }

                finally { threadWorking = false; }
            }, null);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            if (!threadWorking) return false;

            if (taskWasPreviouslyQueued)
                if (TryDequeue(task))
                    return base.TryExecuteTask(task);
                else
                    return false;

            return base.TryExecuteTask(task);
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            List<Task> result = new List<Task>();
            foreach (var lists in tasks.Values)
                foreach (Task task in lists)
                    result.Add(task);

            return result;
        }
    }
}
