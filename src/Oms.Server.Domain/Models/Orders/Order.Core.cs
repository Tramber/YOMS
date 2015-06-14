using System;
using System.Collections.Generic;
using Oms.Server.Domain.Interfaces;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Interfaces.Repository;
using Oms.Server.Domain.Models.EventLogs;
using Oms.Server.Domain.Models.Users;

namespace Oms.Server.Domain.Models.Orders
{
    public partial class Order : IIdentifiable, IOrderCoreData
    {
        public Order()
        {
            
        }

        public int Id { get; set; }

        public IList<EventLog<OrderStateMachine.Trigger>> EventLogs { get; internal set; }

        public User Owner { get; internal set; }

        public User Creator { get; internal set; }

        public DateTime CreationDate { get; internal set; }

        public OrderBasket InitialOrderBasket { get; internal set; }

        public OrderBasket OrderBasket { get; internal set; }

        public IOrderInitialReferentialData InitialReferentialData { get; internal set; }

        public string OrderClientRef { get; internal set; }

        public string ApplicationOrigin { get; internal set; }
    }
}
