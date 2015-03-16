using System;

namespace Oms.Server.Core.Contracts
{
    public interface ITransportService : IDisposable
    {
        void Start();
        void Stop();
    }
}