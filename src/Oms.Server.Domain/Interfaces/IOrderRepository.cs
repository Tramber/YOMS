using System.Collections.Generic;
using Oms.Server.Domain.Orders;

namespace Oms.Server.Domain.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {
        Order GetOrderById(int id);
        IList<Order> GetOrderList();
    }
}