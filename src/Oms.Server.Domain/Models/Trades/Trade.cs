using Oms.Server.Domain.Interfaces.Repository;

namespace Oms.Server.Domain.Models.Trades
{
    public class Trade : IIdentifiable
    {
        public int Id { get; set; }

        public double ExecutionQuantity { get; set; }

        public double ExecutionAmountNet { get; set; }
    }
}
