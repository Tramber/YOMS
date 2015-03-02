using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Security.Tokens;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Microsoft.Practices.Unity;
using Oms.Transport.Wcf;

namespace Oms.Server.Core.Services
{
    public interface ITransportService : IDisposable
    {
        void Start();
        void Stop();
    }

    public class TransportService : ITransportService
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Task _serviceTask;
        private readonly CancellationTokenSource _tokenSource;

        public TransportService(IUnityContainer unityContainer)
        {
            _tokenSource = new CancellationTokenSource();
            _serviceTask = new Task(
                () =>
                {
                    //using (var serviceHost = new UnityServiceHost(unityContainer, typeof(OrderManagementService)))
                    using (var serviceHost = new ServiceHost(typeof(OrderManagementService)))
                    {
                        serviceHost.Open();
                        Console.WriteLine("WCF Service Started");
                        for (; ; )
                        {
                        }
                    }
                },
                _tokenSource.Token,
                TaskCreationOptions.LongRunning);
        }

        public void Start()
        {
            _serviceTask.Start();
        }

        public void Stop()
        {
            Console.WriteLine("WCF Service cancelling");
            _tokenSource.Cancel();
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
