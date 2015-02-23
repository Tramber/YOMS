using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oms.Server.Domain.Interfaces;
using Oms.Server.Domain.Users;

namespace Oms.Server.Domain.Orders
{
    public class OrderGroup : IIdentifiable
    {
        public int Id { get; set; }
        public List<Order> Orders { get; set; }
        public User Creator { get; set; }
    }
}
