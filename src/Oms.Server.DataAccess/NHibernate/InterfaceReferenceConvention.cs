using System;
using System.Collections.Generic;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.EventLogs;
using Oms.Server.Domain.Models.Funds;
using Oms.Server.Domain.Models.Orders;
using Oms.Server.Domain.Models.Securities;
using Oms.Server.Domain.Models.Trades;
using Oms.Server.Domain.Models.Users;

namespace Oms.Server.DataAccess.NHibernate
{
    /// <summary>
    /// The domain exposes its associations as interfaces (<c>Order.Creator</c> is an <c>IUser</c>), which NHibernate cannot
    /// instantiate. This convention points each many-to-one to the single entity class that implements the interface.
    /// </summary>
    public class InterfaceReferenceConvention : IReferenceConvention
    {
        public static readonly IReadOnlyDictionary<Type, Type> Implementations = new Dictionary<Type, Type>
        {
            { typeof(IUser), typeof(User) },
            { typeof(IFund), typeof(Fund) },
            { typeof(ISecurity), typeof(Security) },
            { typeof(IOrder), typeof(Order) },
            { typeof(ITrade), typeof(Trade) },
            { typeof(IOrderBasket), typeof(OrderBasket) },
            { typeof(IOrderInitialReferentialData), typeof(OrderInitialReferentialData) },
            { typeof(ITriggerContext), typeof(TriggerContext) },
            { typeof(IOrderEditableData), typeof(OrderEditableEventParameter) },
            { typeof(IOrderDealingData), typeof(OrderDealingEventParameter) },
            { typeof(ITradeEditableData), typeof(TradeEditableEventParameter) },
        };

        // Created together with the order and owned by it: saved with it. The other references (users, funds,
        // securities, ...) are referential data that must already exist.
        private static readonly HashSet<Type> OwnedByTheReferrer = new HashSet<Type>
        {
            typeof(OrderBasket), typeof(OrderInitialReferentialData), typeof(TriggerContext),
            typeof(OrderEditableEventParameter), typeof(OrderDealingEventParameter), typeof(TradeEditableEventParameter), typeof(Trade),
        };

        public void Apply(IManyToOneInstance instance)
        {
            Type implementation;
            if (!Implementations.TryGetValue(instance.Class.GetUnderlyingSystemType(), out implementation))
                return;
            instance.CustomClass(implementation);
            // The event log subclasses share one table, and each has its own Parameters type: one column (and foreign key) per type.
            if (instance.Name == "Parameters")
                instance.Column(implementation.Name + "Id");
            if (OwnedByTheReferrer.Contains(implementation))
                instance.Cascade.SaveUpdate();
        }
    }
}
