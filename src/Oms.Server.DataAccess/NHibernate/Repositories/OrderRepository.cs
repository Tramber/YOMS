using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oms.Server.Domain.Interfaces;
using Oms.Server.Domain.Orders;

namespace Oms.Server.DataAccess.NHibernate.Repositories
{
    internal class OrderRepository : RepositoryBase<Order>, IOrderRepository
    {
        public Order GetOrderById(int id)
        {
            return base.GetById(id);
        }

        public IList<Order> GetOrderList()
        {
            return base.GetByColumn("IsActive", true);
        }
    }
}
