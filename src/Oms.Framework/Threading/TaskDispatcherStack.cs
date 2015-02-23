using System.Collections.Concurrent;

namespace Oms.Framework.Threading
{
    public class TaskDispatcherStack : TaskDispatcherCollection<ConcurrentStack<TaskDispatcherWorkItem>>
    {
        public TaskDispatcherStack(int workerCount)
            : base(workerCount)
        {
        }
    }
}