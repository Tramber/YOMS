using System.Reflection;
using System.ServiceModel;
using log4net;
using Microsoft.Practices.Unity;
using Oms.Server.Core.Services;
using Oms.Transport.Contracts;

namespace Oms.Server.Core.Contracts
{
    public class TransportService : ITransportService
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ServiceHost serviceHost;

        public TransportService(IOrderServiceContract orderServiceContract)
        {
            serviceHost = new ServiceHost(orderServiceContract);
        }

        public void Start()
        {
            serviceHost.Open();
            Logger.Info("WCF Service Started");
        }

        public void Stop()
        {
            serviceHost.Close();
            Logger.Info("WCF Service Stopped");
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
