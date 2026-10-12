using Oms.Server.Domain.Interfaces.Models;

namespace Oms.Server.Domain.Models.Orders
{
    public static class OrderExtensions
    {


        /// <summary>Copies the core data (creator, owner, dates, references, ...) onto the order. Called when the order is first created. Does nothing when there is no core data.</summary>
        public static Order InsertDataFrom(this Order order, IOrderCoreData coreData)
        {
            if (order != null && coreData != null)
            {
                order.Owner = coreData.Owner;
                order.Creator = coreData.Creator;
                order.CreationDate = coreData.CreationDate;
                order.OrderBasket = coreData.OrderBasket;
                order.InitialReferentialData = coreData.InitialReferentialData;
                order.ClientOrderRef = coreData.ClientOrderRef;
                order.IsAutoAccepted = coreData.IsAutoAccepted;
            }
            return order;
        }

    }
}
