using System;
using System.Threading;
using System.Threading.Tasks;

namespace Oms.Framework.Threading
{
    public class TaskDispatcherWorkItem(
        TaskCompletionSource<object> taskSource,
        Action action,
        CancellationToken? cancelToken)
    {
        public readonly TaskCompletionSource<object> TaskSource = taskSource;
        public readonly Action Action = action;
        public readonly CancellationToken? CancelToken = cancelToken;
    }
}
