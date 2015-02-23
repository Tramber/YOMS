using System;
using System.Threading;
using System.Threading.Tasks;

namespace Oms.Framework.Threading
{
    public interface ITaskDispatcher : IDisposable
    {
        Task AddTask(Action action);
        Task AddTask(Action action, CancellationToken? cancelToken);
    }
}