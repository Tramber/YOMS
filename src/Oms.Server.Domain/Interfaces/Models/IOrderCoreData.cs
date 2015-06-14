using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Oms.Server.Domain.Models.Orders;
using Oms.Server.Domain.Models.Users;

namespace Oms.Server.Domain.Interfaces.Models
{
    public interface IOrderCoreData
    {
        User Owner { get; }

        User Creator { get; }

        DateTime CreationDate { get; }

        OrderBasket InitialOrderBasket { get; }

        OrderBasket OrderBasket { get; }

        IOrderInitialReferentialData InitialReferentialData { get; }

        string OrderClientRef { get; }

        string ApplicationOrigin { get; }

    }
}
