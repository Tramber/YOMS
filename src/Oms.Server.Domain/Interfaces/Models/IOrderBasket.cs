using System.Collections.Generic;
using Oms.Server.Domain.Interfaces.Repository;
using Oms.Server.Domain.Models.Orders;
using Oms.Server.Domain.Models.Users;

namespace Oms.Server.Domain.Interfaces.Models
{
    public interface IOrderBasket : IIdentifiable
    {
        List<Order> Orders { get; set; }
        User Creator { get; set; }
    }
}