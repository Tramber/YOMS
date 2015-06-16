using System.Security.Principal;
using Oms.Server.Domain.Framework;
using Oms.Server.Domain.Interfaces.Repository;
using Oms.Server.Domain.Models.Orders;

namespace Oms.Server.Domain.Models.Trades
{
    public class Trade : IIdentifiable
    {
        public int Id { get; set; }

        public double ExecutionQuantity { get; set; }

        public double ExecutionAmountNet { get; set; }

        public TradeStatus TradeStatus { get; set; }

        public Side Side { get; set; }

        public GenericResult Cancel()
        {
            return GenericResult.Success();
        }

        public GenericResult Update()
        {
            throw new System.NotImplementedException();
        }
    }
}
