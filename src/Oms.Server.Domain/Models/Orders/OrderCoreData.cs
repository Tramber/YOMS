using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.Users;

namespace Oms.Server.Domain.Models.Orders
{
    public class OrderCoreData : IOrderCoreData
    {
        public User Owner { get; set; }
        public User Creator { get; set; }
        public DateTime CreationDate { get; set; }
        public OrderBasket InitialOrderBasket { get; set; }
        public OrderBasket OrderBasket { get; set; }
        public IOrderInitialReferentialData InitialReferentialData { get; set; }
        public string OrderClientRef { get; set; }
        public string ApplicationOrigin { get; set; }
    }
}
