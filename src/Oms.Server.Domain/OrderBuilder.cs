using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oms.Server.Domain.Framework;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.EventLogs;
using Oms.Server.Domain.Models.Funds;
using Oms.Server.Domain.Models.Orders;
using Oms.Server.Domain.Models.Securities;
using Oms.Server.Domain.Models.Trades;
using Oms.Server.Domain.Models.Users;
using Oms.Server.Domain.Workflow;

namespace Oms.Server.Domain
{
    public class OrderBuilder
    {
        protected OrderCoreData _orderCoreData = new OrderCoreData();
        protected OrderEditableEventParameter _editableData = new OrderEditableEventParameter();

        public OrderBuilder WithCoreData(IUser creator, IUser owner, OrderBasket orderBasket, string orderClientRef)
        {
            _orderCoreData.Creator = creator;
            _orderCoreData.CreationDate = DateTime.Now;
            _orderCoreData.ClientOrderRef = orderClientRef;
            _orderCoreData.OrderBasket = orderBasket;
            _orderCoreData.Owner = owner;
            return this;
        }

        public OrderBuilder WithSecurity(ISecurity security)
        {
            _editableData.Security = security;
            return this;
        }

        public OrderBuilder WithFund(IFund fund)
        {
            _editableData.Fund = fund;
            return this;
        }

        public OrderBuilder WithPrices(OrderType orderType, double quantity, double? priceLimit, double? priceStop)
        {
            _editableData.OrderType = orderType;
            _editableData.PriceLimit = priceLimit;
            _editableData.PriceStop = priceStop;
            _editableData.Quantity = quantity;
            return this;
        }

        public OrderBuilder WithDates(DateTime? settlementDate, OrderValidityType orderValidity, DateTime? expiryDate)
        {
            _editableData.SettlementDate = settlementDate;
            _editableData.ExpiryDate = expiryDate;
            _editableData.OrderValidity = orderValidity;
            _editableData.ExpiryDate = expiryDate;
            return this;
        }

        public OrderBuilder WithBasket(OrderBasket orderBasket)
        {
            _orderCoreData.OrderBasket = orderBasket;
            return this;
        }

        public OrderBuilder WithInitialReferentialData(
            string instrumentCode,
            SecurityCodeType securityCodeType,
            string fundCode,
            FundCodeType fundCodeType,
            string fundFolioCode)
        {
            _orderCoreData.InitialReferentialData = new OrderInitialReferentialData
            {
                FundCode = fundCode,
                FundCodeType = fundCodeType,
                FundFolioCode = fundFolioCode,
                InstrumentCode = instrumentCode,
                SecurityCodeType = securityCodeType
            };
            return this;
        }

        public OrderCoreData BuildOrderCoreData()
        {
            return _orderCoreData;
        }

        public IOrderEditableData BuildOrderEditableData()
        {
            return _editableData;
        }

        public virtual Order Build()
        {
            return new Order();
        }

    }
}
