using System.Collections.Generic;
using Oms.Server.Domain.Interfaces;
using Oms.Server.Domain.Interfaces.Repository;
using Oms.Server.Domain.Models.Users;

namespace Oms.Server.Domain.Models.Orders
{
    public class OrderBasket : IIdentifiable
    {
        public int Id { get; set; }
        public List<Order> Orders { get; set; }
        public User Creator { get; set; }
    }
}
