using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oms.Server.Domain.Interfaces;

namespace Oms.Server.DataAccess.NHibernate.Repositories
{
    public class RepositoryFacade : IRepositoryFacade
    {
        public RepositoryFacade()
        {
            Users = new UserRepository();
            Orders = new OrderRepository();
            Assets = new AssetRepository();
        }

        public IUserRepository Users { get; private set; }

        public IOrderRepository Orders { get; private set; }

        public IAssetRepository Assets { get; private set; }
    }
}
