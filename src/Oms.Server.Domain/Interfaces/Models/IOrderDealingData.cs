using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Oms.Server.Domain.Interfaces.Models
{
    interface IOrderDealingData
    {
        double? RemainingQuantity { get; }
    }
}
