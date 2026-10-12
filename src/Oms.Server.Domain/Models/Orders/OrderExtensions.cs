using Oms.Server.Domain.Interfaces.Models;

namespace Oms.Server.Domain.Models.Orders
{
    public static class OrderExtensions
    {


        /// <summary>Copies the core data (creator, owner, dates, references, ...) onto the order. Called when the order is first created.</summary>
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
