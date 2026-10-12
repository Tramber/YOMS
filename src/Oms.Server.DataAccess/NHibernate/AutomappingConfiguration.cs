using System;
using System.Collections.Generic;
using FluentNHibernate.Automapping;
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
            _mappedTypes = new HashSet<Type>
            {
                typeof (Order),
                typeof (OrderBasket),
                typeof (Trade),
                typeof (User),
                typeof (Fund),
                typeof (Security),
                typeof (TriggerContext),
                typeof (OrderEventLog),
                typeof (OrderParameterEventLog<OrderDealingEventParameter>),
                typeof (OrderParameterEventLog<OrderEditableEventParameter>),
                typeof (TradeEventLog),
                typeof (TradeParameterEventLog<TradeEditableEventParameter>),
                typeof (OrderDealingEventParameter),
                typeof (OrderEditableEventParameter),
            };

            _componentTypes = new HashSet<Type>
            {
                typeof (TriggerContext),
                typeof (OrderDealingEventParameter),
                typeof (OrderEditableEventParameter),
                typeof (TradeEditableEventParameter)
            };

            _isDiscriminatedTypes = new HashSet<Type>
            {
                typeof (OrderEventLog),
            };
        }

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
            return type.IsSubclassOf(typeof(OrderEventLog)) || type.IsSubclassOf(typeof(TradeEventLog));
        }
    }
}
