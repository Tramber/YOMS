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
        IUser Owner { get; }

        IUser Creator { get; }

        DateTime CreationDate { get; }

        IOrderBasket OrderBasket { get; }

        IOrderInitialReferentialData InitialReferentialData { get; }

        string ClientOrderRef { get; }

        bool IsAutoAccepted { get; }
    }
}
