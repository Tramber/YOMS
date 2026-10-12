using System.Collections.Concurrent;

namespace Oms.Framework.Threading
{
    public class TaskDispatcherQueue(int workerCount)
        : TaskDispatcherCollection<ConcurrentQueue<TaskDispatcherWorkItem>>(workerCount);
}
