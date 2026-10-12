using Oms.Server.Domain.Interfaces.Repository;

namespace Oms.Server.DataAccess.NHibernate.Repositories
{
    /// <summary>NHibernate repositories behind <see cref="IRepositoryFacade"/>.</summary>
    public class RepositoryFacade : IRepositoryFacade
    {
        public RepositoryFacade()
        {
            Users = new UserRepository();
            Orders = new OrderRepository();
            Securities = new SecurityRepository();
            Funds = new FundRepository();
        }

        public IUserRepository Users { get; private set; }

        public IOrderRepository Orders { get; private set; }

        public ISecurityRepository Securities { get; private set; }
        public IFundRepository Funds { get; private set; }
    }
}
