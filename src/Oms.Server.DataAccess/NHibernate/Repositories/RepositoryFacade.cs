using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oms.Server.Domain.Interfaces;
using Oms.Server.Domain.Interfaces.Repository;

namespace Oms.Server.DataAccess.NHibernate.Repositories
{
    public class RepositoryFacade : IRepositoryFacade
    {
        public RepositoryFacade()
        {
            Users = new UserRepository();
            Orders = new OrderRepository();
            Securities = new InstrumentRepository();
            Funds = new FundRepository();
        }

        public IUserRepository Users { get; private set; }

        public IOrderRepository Orders { get; private set; }

        public IInstrumentRepository Securities { get; private set; }
        public IFundRepository Funds { get; private set; }
    }
}
