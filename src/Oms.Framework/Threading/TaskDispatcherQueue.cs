using System.Collections.Concurrent;

namespace Oms.Framework.Threading
{
    public class TaskDispatcherQueue : TaskDispatcherCollection<ConcurrentQueue<TaskDispatcherWorkItem>>
    {
        public TaskDispatcherQueue(int workerCount) : base(workerCount)
        {
        }
    }
}