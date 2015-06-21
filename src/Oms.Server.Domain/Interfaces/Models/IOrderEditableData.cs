using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oms.Server.Domain.Models.Funds;
using Oms.Server.Domain.Models.Orders;
using Oms.Server.Domain.Models.Securities;
using Oms.Server.Domain.Models.Users;

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
