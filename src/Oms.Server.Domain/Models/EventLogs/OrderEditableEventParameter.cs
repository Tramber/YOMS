using System;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Interfaces.Repository;
using Oms.Server.Domain.Models.Funds;
using Oms.Server.Domain.Models.Securities;

namespace Oms.Server.Domain.Models.EventLogs
{
    public class OrderEditableEventParameter
        : IOrderEditableData, IIdentifiable
    {
        public double Quantity { get; set; }

        public double? PriceLimit { get; set; }

        public double? PriceStop { get; set; }

        public Side Side { get; set; }

        public ISecurity Security { get; set; }

        public OrderType OrderType { get; set; }

        public OrderValidityType OrderValidity { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public IFund Fund { get; set; }
        public DateTime? SettlementDate { get; set; }
        public int Id { get; set; }
    }
}