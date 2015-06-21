using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oms.Server.Domain.Framework;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.EventLogs;
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
                order.OrderBasket = coreData.OrderBasket;
                order.InitialReferentialData = order.InitialReferentialData;
                order.ClientOrderRef = order.ClientOrderRef;
            }
            return order;
        }

    }
}
