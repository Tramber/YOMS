using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Oms.Server.Domain.Interfaces
{
    public interface IRepositoryFacade
    {
        IUserRepository Users { get; }
        IOrderRepository Orders { get; }
        IAssetRepository Assets { get; }
    }
}
