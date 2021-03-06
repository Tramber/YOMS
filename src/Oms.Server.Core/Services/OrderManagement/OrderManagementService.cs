﻿using System;
using System.CodeDom;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oms.Server.Core.Extensions;
using Oms.Server.Domain;
using Oms.Server.Domain.Framework;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Interfaces.Repository;
using Oms.Server.Domain.Models.EventLogs;
using Oms.Server.Domain.Models.Orders;
using Oms.Transport.Contracts;
using Oms.Transport.Contracts.Dto;
using Oms.Transport.Contracts.Messages;
using Oms.Transport.Contracts.Messages.Orders;

namespace Oms.Server.Core.Services
{
    public class OrderManagementService : IOrderManagementService
    {
        private readonly IRepositoryFacade _repositoryFacade;

        private Dictionary<Enum, Func<Order, ITriggerContext,  IOrderCoreData, IOrderEditableData, IOrderDealingData, GenericResult>> OrderCommandMappings = new Dictionary<Enum, Func<Order, ITriggerContext, IOrderCoreData, IOrderEditableData, IOrderDealingData, GenericResult>>
        {
            {EditionCommand.Stash, (o, c, cd, ed, dd) => o.Create(c, cd, ed)},
            {EditionCommand.Create, (o, c, cd, ed, dd) => o.SendRequest(c, cd, ed)},
            {EditionCommand.Update, (o, c, cd, ed, dd) => o.Update(c, cd, ed)},
            {OrderMarketCommand.MarketSend, (o, c, cd, ed, dd) => o.MarketSend(c, dd)},
            {OrderMarketCommand.MarketCancel, (o, c, cd, ed, dd) => o.MarketCancel(c, dd)},
            {OrderStateCommand.AcceptPending, (o, c, cd, ed, dd) => o.AcceptPending(c)},
            {OrderStateCommand.Cancel, (o, c, cd, ed, dd) => o.Cancel(c)},
            {OrderStateCommand.Delete, (o, c, cd, ed, dd) => o.Delete(c)},
            {OrderStateCommand.Recall, (o, c, cd, ed, dd) => o.Recall(c)},
            {OrderStateCommand.StartBooking, (o, c, cd, ed, dd) => o.StartBooking(c)},
            {OrderStateCommand.RejectPending, (o, c, cd, ed, dd) => o.RejectPending(c)},
        };

        public OrderManagementService(IRepositoryFacade repositoryFacade)
        {
            _repositoryFacade = repositoryFacade;
        }

        public GenericResult HandleRequest(OrderEditionRequest request)
        {
            Contract.Requires<ArgumentNullException>(request != null, "request != null");
            var requester = _repositoryFacade.Users.GetUserByLogin(request.Token);
            var orderBasket = new OrderBasket();
            return OrderHandleRequest(request.Command, request.ItemList.Select(o => CreateEditableCommandTuple(o, requester, orderBasket)));
        }

        public GenericResult HandleRequest(OrderMarketRequest request)
        {
            Contract.Requires<ArgumentNullException>(request != null, "request != null");
            return OrderHandleRequest(request.Command, request.ItemList.Select(CreateMarketCommandTuple));
        }

        public GenericResult HandleRequest(OrderRequest request)
        {
            Contract.Requires<ArgumentNullException>(request != null, "request != null");
            return OrderHandleRequest(request.Command, request.ItemList.Select(CreateMarketCommandTuple));
        }

        private GenericResult OrderHandleRequest(Enum command, IEnumerable<Tuple<Order, IOrderEditableData, IOrderCoreData, IOrderDealingData>> parameters)
        {
            foreach (var parameter in parameters)
            {
                var result = OrderCommandMappings[command](parameter.Item1, null, parameter.Item3, parameter.Item2, parameter.Item4);
                if(result.IsSuccess())
                    _repositoryFacade.Orders.Update(parameter.Item1);
            }
            return GenericResult.Success();
        }

        private Tuple<Order, IOrderEditableData, IOrderCoreData, IOrderDealingData> CreateMarketCommandTuple(int orderId)
        {
            var order = _repositoryFacade.Orders.GetById(orderId);
            return new Tuple<Order, IOrderEditableData, IOrderCoreData, IOrderDealingData>(order, null, null, null);
        }

        private Tuple<Order, IOrderEditableData, IOrderCoreData, IOrderDealingData> CreateStateCommandTuple(int orderId)
        {
            var order = _repositoryFacade.Orders.GetById(orderId);
            return new Tuple<Order, IOrderEditableData, IOrderCoreData, IOrderDealingData>(order, null, null, null);
        }

        private Tuple<Order, IOrderEditableData, IOrderCoreData, IOrderDealingData> CreateEditableCommandTuple(OrderDto orderDto, IUser requester, OrderBasket orderBasket)
        {
            var owner = _repositoryFacade.Users.GetById(orderDto.Owner == null ? 0 : orderDto.Owner.Id);
            var security = _repositoryFacade.Securities.GetById(orderDto.Security == null ? 0 : orderDto.Security.Id);
            var fund = _repositoryFacade.Funds.GetById(orderDto.Fund == null ? 0 : orderDto.Fund.Id);
            var orderBuilder = new OrderBuilder()
                .WithCoreData(requester, owner ?? requester, orderBasket, orderDto.ClientOrderRef)
                .WithFund(fund)
                .WithSecurity(security)
                .WithPrices(orderDto.OrderType.ToMappingEnum<OrderType>(), orderDto.Quantity, orderDto.PriceLimit, orderDto.PriceStop)
                .WithDates(orderDto.SettlementDate, orderDto.Validity.ToMappingEnum<OrderValidityType>(), orderDto.ExpiryDate);
            return new Tuple<Order, IOrderEditableData, IOrderCoreData, IOrderDealingData>(new Order(), orderBuilder.BuildOrderEditableData(), orderBuilder.BuildOrderCoreData(), null);
        }
    }
}
