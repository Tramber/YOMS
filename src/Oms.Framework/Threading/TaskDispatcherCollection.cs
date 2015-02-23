using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Oms.Framework.Threading
{
    public abstract class TaskDispatcherCollection<T> : ITaskDispatcher where T : IProducerConsumerCollection<TaskDispatcherWorkItem>, new()
    {
        private readonly BlockingCollection<TaskDispatcherWorkItem> _taskList = new BlockingCollection<TaskDispatcherWorkItem>(new T());
        protected TaskDispatcherCollection(int workerCount)
        {
            for (var i = 0; i < workerCount; i++)
                Task.Factory.StartNew(Consume);
        }

        public void Dispose()
        {
            _taskList.CompleteAdding();
        }

        public Task AddTask(Action action)
        {
            return AddTask(action, null);
        }

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
