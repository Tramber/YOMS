using System.Collections.Concurrent;

namespace Oms.Framework.Threading
{
    public class TaskDispatcherStack(int workerCount)
        : TaskDispatcherCollection<ConcurrentStack<TaskDispatcherWorkItem>>(workerCount);
}
