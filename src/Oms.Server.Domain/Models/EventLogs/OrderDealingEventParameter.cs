using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Interfaces.Repository;
using Oms.Server.Domain.Models.Trades;

namespace Oms.Server.Domain.Models.EventLogs
{
    public class OrderDealingEventParameter : IOrderDealingData, IIdentifiable
    {
        internal OrderDealingEventParameter()
        {
        }

        public OrderDealingEventParameter(ITrade trade)
        {
            Trade = trade;
        }

        public ITrade Trade { get; internal set; }
        public int Id { get; set; }
    }
}