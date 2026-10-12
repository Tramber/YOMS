using System;

namespace Oms.Server.Domain.Interfaces.Models
{
    public interface IOrderEditableData
    {
        double Quantity { get; }

        double? PriceLimit { get; }

        Side Side { get; }

        ISecurity Security { get; }

        OrderType OrderType { get; }

        OrderValidityType OrderValidity { get; }

        DateTime? ExpiryDate { get; }

        IFund Fund { get; }
        double? PriceStop { get; }
        DateTime? SettlementDate { get; }
    }
}
