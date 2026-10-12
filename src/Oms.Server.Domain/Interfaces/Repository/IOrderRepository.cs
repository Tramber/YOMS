using System.Collections.Generic;
using Oms.Server.Domain.Models.Orders;

namespace Oms.Server.Domain.Interfaces.Repository
{
    public interface IOrderRepository : IRepository<Order>
    {
        Order GetOrderById(int id);
        /// <summary>Every order, with its event log.</summary>
        IList<Order> GetOrderList();
    }
}