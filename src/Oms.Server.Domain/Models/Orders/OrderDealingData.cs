using Oms.Server.Domain.Interfaces.Models;

namespace Oms.Server.Domain.Models.Orders
{
    public class OrderDealingData : IOrderDealingData
    {
        public double? RemainingQuantity { get; private set; }
    }
}