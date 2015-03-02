using System.ComponentModel.Design;
using System.Reflection;
using System.Threading;
using System.Web;
using log4net;
using Microsoft.Practices.Unity;
using Oms.Server.Core.Services;
using Oms.Transport.Contracts;

namespace Oms.Server.host
{
    public class HostedGuest
    {
        private readonly static ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly UnityContainer _container;

        public HostedGuest()
        {
            _container = new UnityContainer();
            RegisterServices();
        }

        public void Start()
        {
            Logger.Info("Starting the Server");
            _container.Resolve<ITransportService>().Start();
        }

        private void RegisterServices()
        {
            _container
                .RegisterType<ITransportService, TransportService>(new ContainerControlledLifetimeManager())
                .RegisterType<IOrderServiceContract, OrderServiceContract>();
        }

        public void Stop()
        {
            _container.Resolve<ITransportService>().Stop();
        }
    }
}