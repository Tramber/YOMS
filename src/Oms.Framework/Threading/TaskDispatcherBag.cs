using System.Collections.Concurrent;

namespace Oms.Framework.Threading
{
    public class TaskDispatcherBag(int workerCount)
        : TaskDispatcherCollection<ConcurrentBag<TaskDispatcherWorkItem>>(workerCount);
}
