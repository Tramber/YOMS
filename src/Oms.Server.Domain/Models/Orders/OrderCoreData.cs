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
        public IUser Owner { get; set; }
        public IUser Creator { get; set; }
        public DateTime CreationDate { get; set; }
        public IOrderBasket OrderBasket { get; set; }
        public IOrderInitialReferentialData InitialReferentialData { get; set; }
        public string ClientOrderRef { get; set; }
        public bool IsAutoAccepted { get; set; }
    }
}
