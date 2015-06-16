using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.Trades;

namespace Oms.Server.Domain.Models.Orders
{
    public class OrderDealingData : IOrderDealingData
    {
        public OrderDealingData()
        {
        }

        public OrderDealingData(Trade trade)
        {
            Trade = trade;
        }

        public Trade Trade { get; internal set; }
    }
}