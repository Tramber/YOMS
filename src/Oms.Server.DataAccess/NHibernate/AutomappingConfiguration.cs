using System;
using System.Collections.Generic;
using FluentNHibernate.Automapping;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.EventLogs;
using Oms.Server.Domain.Models.Funds;
using Oms.Server.Domain.Models.Orders;
using Oms.Server.Domain.Models.Securities;
using Oms.Server.Domain.Models.Trades;
using Oms.Server.Domain.Models.Users;

namespace Oms.Server.DataAccess.NHibernate
{
    public class AutomappingConfiguration : DefaultAutomappingConfiguration
    {
        private readonly HashSet<Type> _mappedTypes;
        private readonly HashSet<Type> _componentTypes;
        private readonly HashSet<Type> _isDiscriminatedTypes;

        public AutomappingConfiguration()
        {
            _mappedTypes =
            [
                typeof(Order),
                typeof(OrderBasket),
                typeof(OrderInitialReferentialData),
                typeof(Trade),
                typeof(User),
                typeof(Fund),
                typeof(Security),
                typeof(TriggerContext),
                typeof(OrderEventLog),
                typeof(OrderParameterEventLog<IOrderDealingData>),
                typeof(OrderParameterEventLog<IOrderEditableData>),
                typeof(TradeEventLog),
                typeof(TradeParameterEventLog<ITradeEditableData>),
                typeof(OrderDealingEventParameter),
                typeof(OrderEditableEventParameter),
                typeof(TradeEditableEventParameter)
            ];

            _componentTypes = [];

            _isDiscriminatedTypes =
            [
                typeof(OrderEventLog)
            ];
        }

        /// <summary>The closed generic event logs; assembly scanning cannot find them.</summary>
        public static readonly Type[] ClosedGenericTypes =
        [
            typeof (OrderParameterEventLog<IOrderDealingData>),
            typeof (OrderParameterEventLog<IOrderEditableData>),
            typeof (TradeParameterEventLog<ITradeEditableData>)
        ];

        public override string GetDiscriminatorColumn(Type type)
        {
            return "type";
        }

        public override bool IsComponent(Type type)
        {
            return _componentTypes.Contains(type);
        }

        public override bool ShouldMap(Type type)
        {
            return _mappedTypes.Contains(type);
        }

        public override bool IsDiscriminated(Type type)
        {
            // Asked for the root of a hierarchy, not for the subclass: each event log hierarchy is one table with a discriminator.
            return typeof(OrderEventLog).IsAssignableFrom(type) || typeof(TradeEventLog).IsAssignableFrom(type);
        }
    }
}
