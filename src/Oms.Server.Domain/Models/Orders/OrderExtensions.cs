using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oms.Server.Domain.Interfaces.Models;

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
    }
}
