using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oms.Server.Domain.Models.Funds;
using Oms.Server.Domain.Models.Instruments;
using Oms.Server.Domain.Models.Orders;
using Oms.Server.Domain.Models.Users;

namespace Oms.Server.Domain.Interfaces.Models
{
    public interface IOrderTransientData
    {
        double Quantity { get; }

        double Price { get; }

        Side Side { get; }

        Instrument Instrument { get; }

        OrderType OrderType { get; }

        OrderValidityType OrderValidity { get; }

        DateTime? ExpiryDate { get; }

        Fund Fund { get; }
    }
}
