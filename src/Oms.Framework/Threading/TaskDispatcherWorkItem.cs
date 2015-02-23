using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Oms.Framework.Threading
{
    public class TaskDispatcherWorkItem
    {
        public readonly TaskCompletionSource<object> TaskSource;
        public readonly Action Action;
        public readonly CancellationToken? CancelToken;

        public TaskDispatcherWorkItem(TaskCompletionSource<object> taskSource, Action action, CancellationToken? cancelToken)
        {
            TaskSource = taskSource;
            Action = action;
            CancelToken = cancelToken;
        }
    }
}