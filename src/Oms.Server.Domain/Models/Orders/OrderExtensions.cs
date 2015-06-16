using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oms.Server.Domain.Framework;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.Trades;
using Oms.Server.Domain.Workflow;

namespace Oms.Server.Domain.Models.Orders
{
    public static class OrderExtensions
    {


        public static Order InsertDataFrom(this Order order, IOrderCoreData coreData)
        {
            if (order != null)
            {
                //TODO handle coreData is null ?
                order.Owner = coreData.Owner;
                order.Creator = coreData.Creator;
                order.CreationDate = coreData.CreationDate;
                order.InitialOrderBasket = coreData.InitialOrderBasket;
                order.OrderBasket = coreData.OrderBasket;
                order.InitialReferentialData = order.InitialReferentialData;
                order.OrderClientRef = order.OrderClientRef;
                order.ApplicationOrigin = order.ApplicationOrigin;
            }
            return order;
        }

        public static GenericResult ForwardCommand(this OrderStateMachine.Trigger orderTrigger, Trade trade)
        {
            if (trade == null)
            {
                return GenericResult.FailureFormat("Could not follow an empty trade with command {0}", orderTrigger);
            }
            switch (orderTrigger)
            {
                case OrderStateMachine.Trigger.CancelTrade:
                    return trade.Cancel();
                case OrderStateMachine.Trigger.AddTrade:
                    return GenericResult.Success();
                case OrderStateMachine.Trigger.UpdateTrade:
                    return trade.Update();
                default:
                    return GenericResult.FailureFormat("The order command {0} does not require trade operation", orderTrigger);
            }
        }
    }
}
