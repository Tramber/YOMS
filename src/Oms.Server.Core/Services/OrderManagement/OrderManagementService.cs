using System;
using System.Collections.Generic;
using System.Linq;
using Oms.Server.Core.Extensions;
using Oms.Server.Domain;
using Oms.Server.Domain.Framework;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Interfaces.Repository;
using Oms.Server.Domain.Models.EventLogs;
using Oms.Server.Domain.Models.Orders;
using Oms.Server.Domain.Models.Securities;
using Oms.Server.Domain.Models.Trades;
using Oms.Transport.Contracts.Dto;
using Oms.Transport.Contracts.Messages;
using Oms.Transport.Contracts.Messages.Orders;

namespace Oms.Server.Core.Services
{
    /// <summary>Application service behind the order requests: loads or builds the orders, applies the requested command and saves the ones that succeeded.</summary>
    public class OrderManagementService : IOrderManagementService
    {
        private readonly IRepositoryFacade _repositoryFacade;

        private Dictionary<Enum, Func<Order, ITriggerContext,  IOrderCoreData, IOrderEditableData, IOrderDealingData, GenericResult>> _orderCommandMappings = new Dictionary<Enum, Func<Order, ITriggerContext, IOrderCoreData, IOrderEditableData, IOrderDealingData, GenericResult>>
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

        /// <summary>
        /// Creates (Stash, Create) or updates (Update) the orders described by the request. A created order is saved, an
        /// updated one is loaded by its id; the DTO's data goes through the usual validation.
        /// </summary>
        public IList<OrderCommandResult> HandleRequest(OrderEditionRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            var requester = _repositoryFacade.Users.GetUserByLogin(request.Token);
            var orderBasket = new OrderBasket();
            return request.ItemList
                .Select(dto => Execute(requester, request.Command, dto.Id, () => EditionTarget(request.Command, dto, requester, orderBasket)))
                .ToList();
        }

        /// <summary>Applies a market command to the existing orders listed in the request.</summary>
        public IList<OrderCommandResult> HandleRequest(OrderMarketRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            var requester = _repositoryFacade.Users.GetUserByLogin(request.Token);
            return request.ItemList
                .Select(id => Execute(requester, request.Command, id, () => new Target { Order = _repositoryFacade.Orders.GetById(id), Dealing = new OrderDealingEventParameter(null) }))
                .ToList();
        }

        /// <summary>Applies a state command (cancel, recall, delete, ...) to the existing orders listed in the request.</summary>
        public IList<OrderCommandResult> HandleRequest(OrderRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            var requester = _repositoryFacade.Users.GetUserByLogin(request.Token);
            return request.ItemList
                .Select(id => Execute(requester, request.Command, id, () => new Target { Order = _repositoryFacade.Orders.GetById(id) }))
                .ToList();
        }

        /// <summary>
        /// Adds a trade to an order, cancels one or books one. Booking a trade that was not sent to a booking system
        /// forces it to booked first; the order terminates with its last booked trade.
        /// </summary>
        public IList<OrderCommandResult> HandleRequest(OrderTradeRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            var requester = _repositoryFacade.Users.GetUserByLogin(request.Token);
            return request.ItemList.Select(dto => ExecuteTrade(requester, request.Command, dto)).ToList();
        }

        /// <summary>Every order, with its event log.</summary>
        public IList<Order> GetOrders()
        {
            return _repositoryFacade.Orders.GetOrderList();
        }

        /// <summary>The active securities an order can be placed on (those the DTOs can describe).</summary>
        public IList<Security> GetSecurities()
        {
            return _repositoryFacade.Securities.GetActiveList()
                .Where(s => s.Currency != Currency.Undefined && s.Type != SecurityType.Undefined)
                .ToList();
        }

        public IList<IFund> GetFunds()
        {
            return _repositoryFacade.Funds.GetAll();
        }

        private OrderCommandResult Execute(IUser requester, Enum command, int orderId, Func<Target> prepare)
        {
            if (requester == null)
                return new OrderCommandResult(orderId, null, GenericResult.Failure("Unknown user"));

            Target target;
            try
            {
                target = prepare();
            }
            catch (NotSupportedException ex)
            {
                // A DTO value the domain has no equivalent for.
                return new OrderCommandResult(orderId, null, GenericResult.Failure(ex.Message));
            }
            if (target.Order == null)
                return new OrderCommandResult(orderId, null, GenericResult.FailureFormat("Order {0} not found", orderId));

            var result = _orderCommandMappings[command](target.Order, new TriggerContext(requester), target.Core, target.Editable, target.Dealing);
            if (result.IsSuccess())
            {
                if (target.Order.Id == 0)
                    _repositoryFacade.Orders.Add(target.Order);
                else
                    _repositoryFacade.Orders.Update(target.Order);
            }
            return new OrderCommandResult(target.Order.Id, target.Order, result);
        }

        private OrderCommandResult ExecuteTrade(IUser requester, OrderTradeCommand command, TradeDto dto)
        {
            if (requester == null)
                return new OrderCommandResult(dto.OrderId, null, GenericResult.Failure("Unknown user"));
            var order = _repositoryFacade.Orders.GetById(dto.OrderId);
            if (order == null)
                return new OrderCommandResult(dto.OrderId, null, GenericResult.FailureFormat("Order {0} not found", dto.OrderId));

            var context = new TriggerContext(requester);
            GenericResult result;
            if (command == OrderTradeCommand.AddTrade)
            {
                Side side;
                if (!Enum.TryParse(dto.Way.ToString(), out side))
                    return new OrderCommandResult(order.Id, order, GenericResult.Failure("Unknown way"));
                result = order.AddTrade(context, new Trade { ExecutionQuantity = dto.ExecutionQuantity, ExecutionAmountNet = dto.ExecutionAmountNet, Side = side });
            }
            else
            {
                var trade = order.GetTrades().FirstOrDefault(t => t.Id == dto.Id);
                if (trade == null)
                    return new OrderCommandResult(order.Id, order, GenericResult.FailureFormat("Trade {0} not found on order {1}", dto.Id, order.Id));
                result = command == OrderTradeCommand.CancelTrade ? order.CancelTrade(context, trade) : BookTrade(order, trade, context);
            }
            if (result.IsSuccess())
                _repositoryFacade.Orders.Update(order);
            return new OrderCommandResult(order.Id, order, result);
        }

        private static GenericResult BookTrade(Order order, ITrade trade, ITriggerContext context)
        {
            // No booking system yet: a trade that was not sent to one is booked by hand.
            if (trade.TradeState == TradeState.New || trade.TradeState == TradeState.BookingError)
            {
                var forced = trade.ForceBooked(context);
                if (forced.IsFailure()) return forced;
            }
            return order.TradeBooked(context, trade);
        }

        private Target EditionTarget(EditionCommand command, OrderDto orderDto, IUser requester, OrderBasket orderBasket)
        {
            var owner = _repositoryFacade.Users.GetById(orderDto.Owner == null ? 0 : orderDto.Owner.Id);
            var security = _repositoryFacade.Securities.GetById(orderDto.Security == null ? 0 : orderDto.Security.Id);
            var fund = _repositoryFacade.Funds.GetById(orderDto.Fund == null ? 0 : orderDto.Fund.Id);
            var orderBuilder = new OrderBuilder()
                .WithCoreData(requester, owner ?? requester, orderBasket, orderDto.ClientOrderRef)
                .WithFund(fund)
                .WithSecurity(security)
                .WithSide(orderDto.Way.ToMappingEnum<Side>())
                .WithPrices(orderDto.OrderType.ToMappingEnum<OrderType>(), orderDto.Quantity, orderDto.PriceLimit, orderDto.PriceStop)
                .WithDates(orderDto.SettlementDate, orderDto.Validity.ToMappingEnum<OrderValidityType>(), orderDto.ExpiryDate);
            return new Target
            {
                Order = command == EditionCommand.Update ? _repositoryFacade.Orders.GetById(orderDto.Id) : new Order(),
                Editable = orderBuilder.BuildOrderEditableData(),
                Core = orderBuilder.BuildOrderCoreData(),
            };
        }

        /// <summary>What a command is applied to, and with which data.</summary>
        private sealed class Target
        {
            public Order Order;
            public IOrderEditableData Editable;
            public IOrderCoreData Core;
            public IOrderDealingData Dealing;
        }
    }
}
