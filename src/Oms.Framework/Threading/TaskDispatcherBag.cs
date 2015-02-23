using System.Collections.Concurrent;

namespace Oms.Framework.Threading
{
    public class TaskDispatcherBag : TaskDispatcherCollection<ConcurrentBag<TaskDispatcherWorkItem>>
    {
        public TaskDispatcherBag(int workerCount) : base(workerCount)
        {
        }
    }
}