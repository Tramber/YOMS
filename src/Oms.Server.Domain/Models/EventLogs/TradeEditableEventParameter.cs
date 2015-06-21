using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Interfaces.Repository;

namespace Oms.Server.Domain.Models.EventLogs
{
    public class TradeEditableEventParameter : ITradeEditableData, IIdentifiable
    {
        public double ExecutionQuantity { get; set; }
        public double ExecutionAmountNet { get; set; }
        public TradeState TradeState { get; set; }
        public Side Side { get; set; }
        public int Id { get; set; }
    }

}