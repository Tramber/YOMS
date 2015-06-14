using System.Collections.Generic;
using Oms.Server.Domain.Models.Orders;

namespace Oms.Server.Domain.Interfaces.Repository
{
    public interface IOrderRepository : IRepository<Order>
    {
        Order GetOrderById(int id);
        IList<Order> GetOrderList();
    }
}