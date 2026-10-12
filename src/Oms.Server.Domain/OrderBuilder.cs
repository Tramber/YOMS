using System;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.EventLogs;
using Oms.Server.Domain.Models.Funds;
using Oms.Server.Domain.Models.Orders;
using Oms.Server.Domain.Models.Securities;

namespace Oms.Server.Domain
{
    /// <summary>Collects the data of an order to create. Build the core and editable data for the order commands, or <see cref="Build"/> for an empty order.</summary>
    public class OrderBuilder
    {
        protected OrderCoreData OrderCoreData = new OrderCoreData();
        protected OrderEditableEventParameter EditableData = new OrderEditableEventParameter();

        public OrderBuilder WithCoreData(IUser creator, IUser owner, OrderBasket orderBasket, string orderClientRef)
        {
            OrderCoreData.Creator = creator;
            OrderCoreData.CreationDate = DateTime.Now;
            OrderCoreData.ClientOrderRef = orderClientRef;
            OrderCoreData.OrderBasket = orderBasket;
            OrderCoreData.Owner = owner;
            return this;
        }

        public OrderBuilder WithSecurity(ISecurity security)
        {
            EditableData.Security = security;
            return this;
        }

        public OrderBuilder WithFund(IFund fund)
        {
            EditableData.Fund = fund;
            return this;
        }

        public OrderBuilder WithSide(Side side)
        {
            EditableData.Side = side;
            return this;
        }

        public OrderBuilder WithPrices(OrderType orderType, double quantity, double? priceLimit, double? priceStop)
        {
            EditableData.OrderType = orderType;
            EditableData.PriceLimit = priceLimit;
            EditableData.PriceStop = priceStop;
            EditableData.Quantity = quantity;
            return this;
        }

        public OrderBuilder WithDates(DateTime? settlementDate, OrderValidityType orderValidity, DateTime? expiryDate)
        {
            EditableData.SettlementDate = settlementDate;
            EditableData.ExpiryDate = expiryDate;
            EditableData.OrderValidity = orderValidity;
            EditableData.ExpiryDate = expiryDate;
            return this;
        }

        public OrderBuilder WithBasket(OrderBasket orderBasket)
        {
            OrderCoreData.OrderBasket = orderBasket;
            return this;
        }

        public OrderBuilder WithInitialReferentialData(
            string instrumentCode,
            SecurityCodeType securityCodeType,
            string fundCode,
            FundCodeType fundCodeType,
            string fundFolioCode)
        {
            OrderCoreData.InitialReferentialData = new OrderInitialReferentialData
            {
                FundCode = fundCode,
                FundCodeType = fundCodeType,
                FundFolioCode = fundFolioCode,
                InstrumentCode = instrumentCode,
                SecurityCodeType = securityCodeType
            };
            return this;
        }

        /// <summary>The core data (creator, owner, basket, client reference, initial referential data) set so far.</summary>
        public OrderCoreData BuildOrderCoreData()
        {
            return OrderCoreData;
        }

        /// <summary>The editable data (fund, security, prices, dates) set so far.</summary>
        public IOrderEditableData BuildOrderEditableData()
        {
            return EditableData;
        }

        public virtual Order Build()
        {
            return new Order();
        }

    }
}
