using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using Oms.Server.Domain.Assets;
using Oms.Server.Domain.Interfaces;
using Oms.Server.Domain.Users;

namespace Oms.Server.Domain.Orders
{
    public class Order : IIdentifiable
    {
        public int Id { get; set; }
        public double Quantity { get; set; }
        public double Price { get; set; }
        public OrderWay Way { get; set; }
        public Asset Asset { get; set; }
        public User Owner { get; set; }
        public OrderGroup Group { get; set; }
        public User Creator { get; set; }
        public string Version { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreationDate { get; set; }
        public OrderType OrderType { get; set; }
        public OrderValidityType OrderValidity { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}
