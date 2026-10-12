using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Oms.Framework.Threading
{
    /// <summary>Runs queued actions on a fixed number of worker tasks. The collection type decides the order: queue (FIFO), stack (LIFO) or bag (unordered).</summary>
    public abstract class TaskDispatcherCollection<T> : ITaskDispatcher where T : IProducerConsumerCollection<TaskDispatcherWorkItem>, new()
    {
        private readonly BlockingCollection<TaskDispatcherWorkItem> _taskList = new BlockingCollection<TaskDispatcherWorkItem>(new T());
        protected TaskDispatcherCollection(int workerCount)
        {
            for (var i = 0; i < workerCount; i++)
                Task.Factory.StartNew(Consume);
        }

        /// <summary>Stops accepting tasks. The workers finish what is already queued.</summary>
        public void Dispose()
        {
            _taskList.CompleteAdding();
        }

        public Task AddTask(Action action)
        {
            return AddTask(action, null);
        }

        /// <summary>Queues the action. The returned task completes when it has run, faults if it throws, and is cancelled if the token is cancelled before it starts.</summary>
        public Task AddTask(Action action, CancellationToken? cancelToken)
        {
            var tcs = new TaskCompletionSource<object>();
            _taskList.Add(new TaskDispatcherWorkItem(tcs, action, cancelToken));
            return tcs.Task;
        }

        private void Consume()
        {
            foreach (var workItem in _taskList.GetConsumingEnumerable())
            {
                if (workItem.CancelToken.HasValue && workItem.CancelToken.Value.IsCancellationRequested)
                {
                    workItem.TaskSource.SetCanceled();
                }
                else
                {
                    try
                    {
                        workItem.Action();
                        workItem.TaskSource.SetResult(null);
                    }
                    catch (OperationCanceledException ex)
                    {
                        if (ex.CancellationToken == workItem.CancelToken)
                            workItem.TaskSource.SetCanceled();
                        else
                            workItem.TaskSource.SetException(ex);
                    }
                    catch (Exception ex)
                    {
                        workItem.TaskSource.SetException(ex);
                    }
                }

            }
        }
    }
}
