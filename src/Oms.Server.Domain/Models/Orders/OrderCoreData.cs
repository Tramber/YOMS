using System;
using Oms.Server.Domain.Interfaces.Models;

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
