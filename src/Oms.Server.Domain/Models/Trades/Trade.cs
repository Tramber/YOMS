using System.Security.Principal;
using Oms.Server.Domain.Framework;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Interfaces.Repository;
using Oms.Server.Domain.Models.Orders;

namespace Oms.Server.Domain.Models.Trades
{
    public class Trade : ITrade
    {
        public int Id { get; set; }

        public double ExecutionQuantity { get; set; }

        public double ExecutionAmountNet { get; set; }

        public TradeState TradeState { get; set; }

        public Side Side { get; set; }
        public IOrder Order { get; set; }

        public GenericResult Cancel(ITriggerContext context)
        {
            return GenericResult.Success();
        }

        public GenericResult Update(ITriggerContext context)
        {
            throw new System.NotImplementedException();
        }

        public GenericResult Create(ITriggerContext context, ITradeEditableData editableData)
        {
            throw new System.NotImplementedException();
        }

        public GenericResult Update(ITriggerContext context, ITradeEditableData editableData)
        {
            throw new System.NotImplementedException();
        }
    }
}
