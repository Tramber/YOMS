using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Oms.Server.Core.Services;
using Oms.Server.Domain;
using Oms.Server.Domain.Framework;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Interfaces.Repository;
using Oms.Server.Domain.Models.EventLogs;
using Oms.Server.Domain.Models.Funds;
using Oms.Server.Domain.Models.Orders;
using Oms.Server.Domain.Models.Securities;
using Oms.Server.Domain.Models.Users;
using Oms.Server.Domain.Workflow;
using Oms.Transport.Contracts.Dto;
using Oms.Transport.Contracts.Messages;
using Oms.Transport.Contracts.Messages.Orders;

namespace Oms.Server.Core.Tests.Services
{
    [TestFixture]
    public class OrderManagementServiceTests
    {
        private IRepositoryFacade _repositories;
        private IOrderRepository _orders;
        private IUserRepository _users;
        private ISecurityRepository _securities;
        private IFundRepository _funds;
        private User _user;
        private OrderManagementService _service;

        [SetUp]
        public void SetUp()
        {
            _orders = Substitute.For<IOrderRepository>();
            _users = Substitute.For<IUserRepository>();
            _securities = Substitute.For<ISecurityRepository>();
            _funds = Substitute.For<IFundRepository>();
            _repositories = Substitute.For<IRepositoryFacade>();
            _repositories.Orders.Returns(_orders);
            _repositories.Users.Returns(_users);
            _repositories.Securities.Returns(_securities);
            _repositories.Funds.Returns(_funds);
            _user = new User { Id = 1, Login = "token" };
            _users.GetUserByLogin("token").Returns(_user);
            _securities.GetById(1).Returns(new Security { Id = 1 });
            _funds.GetById(1).Returns(new Fund { Id = 1 });
            _service = new OrderManagementService(_repositories);
        }

        private Order AcceptingOrder(int id)
        {
            var builder = new OrderBuilder()
                .WithCoreData(_user, _user, new OrderBasket(), "ref")
                .WithFund(new Fund())
                .WithSecurity(new Security())
                .WithPrices(OrderType.Limit, 100, 10, null)
                .WithInitialReferentialData("instCode", SecurityCodeType.Isin, "fundCode", FundCodeType.Other, "folioCode");
            var order = new Order { Id = id };
            Assert.That(order.SendRequest(new TriggerContext(_user), builder.BuildOrderCoreData(), builder.BuildOrderEditableData()).IsSuccess(), Is.True);
            return order;
        }

        private Order WorkingOrder(int id)
        {
            var order = AcceptingOrder(id);
            Assert.That(order.SendAccept(new TriggerContext(_user, DateTime.Now.AddMinutes(1))).IsSuccess(), Is.True);
            return order;
        }

        private static OrderRequest Request(OrderStateCommand command, params int[] ids)
        {
            return new OrderRequest("token", command, [.. ids], default(TransactionMode));
        }

        private static OrderDto Dto(double quantity = 100, int id = 0)
        {
            return new OrderDto
            {
                Id = id,
                Quantity = quantity,
                PriceLimit = 10,
                OrderType = OrderDtoType.Limit,
                Validity = OrderDtoValidityType.Day,
                Security = new SecurityDto { Id = 1 },
                Fund = new FundDto { Id = 1 },
                ClientOrderRef = "client-ref",
            };
        }

        private static OrderEditionRequest Edition(EditionCommand command, params OrderDto[] dtos)
        {
            return new OrderEditionRequest("token", command, [.. dtos], default(TransactionMode));
        }

        [Test]
        public void Should_apply_the_command_to_the_loaded_order_and_save_it()
        {
            var order = AcceptingOrder(1);
            _orders.GetById(1).Returns(order);

            var results = _service.HandleRequest(Request(OrderStateCommand.Recall, 1));

            Assert.That(results.Single().Result.IsSuccess(), Is.True, results.Single().Result.ErrorMessage);
            Assert.That(results.Single().OrderId, Is.EqualTo(1));
            Assert.That(results.Single().Order, Is.SameAs(order));
            Assert.That(order.OrderState, Is.EqualTo(OrderStateMachine.State.Draft));
            _orders.Received(1).Update(order);
        }

        [Test]
        public void Should_run_the_command_as_the_requester()
        {
            var order = AcceptingOrder(1);
            _orders.GetById(1).Returns(order);

            _service.HandleRequest(Request(OrderStateCommand.Recall, 1));

            Assert.That(order.EventLogs.Last().Context.User, Is.SameAs(_user));
        }

        [Test]
        public void Should_handle_every_order_of_the_request()
        {
            var first = AcceptingOrder(1);
            var second = AcceptingOrder(2);
            _orders.GetById(1).Returns(first);
            _orders.GetById(2).Returns(second);

            var results = _service.HandleRequest(Request(OrderStateCommand.Recall, 1, 2));

            Assert.That(results.Select(r => r.OrderId), Is.EqualTo([1, 2]));
            Assert.That(results.All(r => r.Result.IsSuccess()), Is.True);
            _orders.Received(1).Update(first);
            _orders.Received(1).Update(second);
        }

        [Test]
        public void Should_report_and_not_save_an_order_when_the_command_is_refused()
        {
            var order = AcceptingOrder(1);
            _orders.GetById(1).Returns(order);

            var results = _service.HandleRequest(Request(OrderStateCommand.Delete, 1));

            Assert.That(results.Single().Result.IsFailure(), Is.True);
            Assert.That(results.Single().Result.ErrorMessage, Does.Contain("not allowed"));
            Assert.That(order.OrderState, Is.EqualTo(OrderStateMachine.State.Accepting));
            _orders.DidNotReceive().Update(Arg.Any<Order>());
        }

        [Test]
        public void Should_report_an_order_that_does_not_exist_and_still_handle_the_others()
        {
            var order = AcceptingOrder(2);
            _orders.GetById(2).Returns(order);

            var results = _service.HandleRequest(Request(OrderStateCommand.Recall, 5, 2));

            Assert.That(results[0].Result.IsFailure(), Is.True);
            Assert.That(results[0].Result.ErrorMessage, Does.Contain("5"));
            Assert.That(results[0].Order, Is.Null);
            Assert.That(results[1].Result.IsSuccess(), Is.True);
            _orders.Received(1).Update(order);
        }

        [Test]
        public void Should_refuse_every_order_when_the_user_is_unknown()
        {
            _users.GetUserByLogin("stranger").Returns((User)null);
            var request = new OrderRequest("stranger", OrderStateCommand.Recall, [1, 2], default(TransactionMode));

            var results = _service.HandleRequest(request);

            Assert.That(results.Select(r => r.OrderId), Is.EqualTo([1, 2]));
            Assert.That(results.All(r => r.Result.IsFailure() && r.Result.ErrorMessage.Contains("Unknown user")), Is.True);
            _orders.DidNotReceive().GetById(Arg.Any<int>());
        }

        [Test]
        public void Should_refuse_a_null_request()
        {
            Assert.Throws<ArgumentNullException>(() => _service.HandleRequest((OrderRequest)null));
            Assert.Throws<ArgumentNullException>(() => _service.HandleRequest((OrderEditionRequest)null));
            Assert.Throws<ArgumentNullException>(() => _service.HandleRequest((OrderMarketRequest)null));
        }

        [Test]
        public void Should_do_nothing_for_an_empty_request()
        {
            Assert.That(_service.HandleRequest(Request(OrderStateCommand.Recall)), Is.Empty);
            _orders.DidNotReceive().Update(Arg.Any<Order>());
        }

        [Test]
        public void Should_create_save_and_send_the_new_order_for_acceptance()
        {
            var results = _service.HandleRequest(Edition(EditionCommand.Create, Dto(100)));

            var result = results.Single();
            Assert.That(result.Result.IsSuccess(), Is.True, result.Result.ErrorMessage);
            Assert.That(result.Order.OrderState, Is.EqualTo(OrderStateMachine.State.Accepting));
            Assert.That(result.Order.Quantity, Is.EqualTo(100));
            Assert.That(result.Order.ClientOrderRef, Is.EqualTo("client-ref"));
            Assert.That(result.Order.Creator, Is.SameAs(_user));
            _orders.Received(1).Add(result.Order);
            _orders.DidNotReceive().Update(Arg.Any<Order>());
        }

        [Test]
        public void Should_stash_a_new_order_as_a_draft()
        {
            var results = _service.HandleRequest(Edition(EditionCommand.Stash, Dto(100)));

            Assert.That(results.Single().Order.OrderState, Is.EqualTo(OrderStateMachine.State.Draft));
            _orders.Received(1).Add(results.Single().Order);
        }

        [Test]
        public void Should_not_save_an_order_with_invalid_data()
        {
            var results = _service.HandleRequest(Edition(EditionCommand.Create, Dto(0)));

            Assert.That(results.Single().Result.IsFailure(), Is.True);
            _orders.DidNotReceive().Add(Arg.Any<Order>());
        }

        [Test]
        public void Should_not_save_an_order_without_a_known_security_or_fund()
        {
            var dto = Dto();
            dto.Security = new SecurityDto { Id = 99 };

            var results = _service.HandleRequest(Edition(EditionCommand.Create, dto));

            Assert.That(results.Single().Result.IsFailure(), Is.True);
            _orders.DidNotReceive().Add(Arg.Any<Order>());
        }

        [Test]
        public void Should_update_the_existing_order_and_wait_for_the_update_to_be_accepted()
        {
            var order = AcceptingOrder(7);
            _orders.GetById(7).Returns(order);

            var results = _service.HandleRequest(Edition(EditionCommand.Update, Dto(200, id: 7)));

            Assert.That(results.Single().Result.IsSuccess(), Is.True, results.Single().Result.ErrorMessage);
            Assert.That(results.Single().OrderId, Is.EqualTo(7));
            Assert.That(order.Quantity, Is.EqualTo(100));
            Assert.That(order.PendingTrigger, Is.EqualTo(OrderStateMachine.Trigger.Update));
            _orders.Received(1).Update(order);
            _orders.DidNotReceive().Add(Arg.Any<Order>());
        }

        [Test]
        public void Should_report_an_update_of_an_order_that_does_not_exist()
        {
            var results = _service.HandleRequest(Edition(EditionCommand.Update, Dto(200, id: 7)));

            Assert.That(results.Single().Result.IsFailure(), Is.True);
            Assert.That(results.Single().Order, Is.Null);
        }

        [Test]
        public void Should_create_the_order_on_the_side_of_the_dto()
        {
            var dto = Dto(100);
            dto.Way = OrderDtoWay.Sell;

            var results = _service.HandleRequest(Edition(EditionCommand.Create, dto));

            Assert.That(results.Single().Result.IsSuccess(), Is.True, results.Single().Result.ErrorMessage);
            Assert.That(results.Single().Order.Side, Is.EqualTo(Side.Sell));
        }

        [Test]
        public void Should_list_the_orders_of_the_repository()
        {
            var orders = new List<Order> { AcceptingOrder(1), AcceptingOrder(2) };
            _orders.GetOrderList().Returns(orders);

            Assert.That(_service.GetOrders(), Is.EqualTo(orders));
        }

        [Test]
        public void Should_only_list_the_active_securities_that_a_dto_can_describe()
        {
            var usable = new Security { Id = 1, Currency = Currency.Eur, Type = SecurityType.Equity };
            _securities.GetActiveList().Returns(new List<Security>
            {
                usable,
                new Security { Id = 2, Currency = Currency.Undefined, Type = SecurityType.Bond },
                new Security { Id = 3, Currency = Currency.Usd, Type = SecurityType.Undefined },
            });

            Assert.That(_service.GetSecurities().Select(s => s.Id), Is.EqualTo([1]));
        }

        [Test]
        public void Should_list_the_funds_of_the_repository()
        {
            var funds = new List<IFund> { new Fund { Id = 1 }, new Fund { Id = 2 } };
            _funds.GetAll().Returns(funds);

            Assert.That(_service.GetFunds(), Is.EqualTo(funds));
        }

        [Test]
        public void Should_send_a_working_order_to_the_market()
        {
            var order = WorkingOrder(3);
            _orders.GetById(3).Returns(order);
            var request = new OrderMarketRequest("token", OrderMarketCommand.MarketSend, [3], default(TransactionMode));

            var results = _service.HandleRequest(request);

            Assert.That(results.Single().Result.IsSuccess(), Is.True, results.Single().Result.ErrorMessage);
            Assert.That(order.OrderState, Is.EqualTo(OrderStateMachine.State.Dealing));
            _orders.Received(1).Update(order);
        }

        [Test]
        public void Should_refuse_to_cancel_at_the_market_an_order_that_was_not_sent()
        {
            var order = WorkingOrder(3);
            _orders.GetById(3).Returns(order);
            var request = new OrderMarketRequest("token", OrderMarketCommand.MarketCancel, [3], default(TransactionMode));

            var results = _service.HandleRequest(request);

            Assert.That(results.Single().Result.IsFailure(), Is.True);
            _orders.DidNotReceive().Update(Arg.Any<Order>());
        }

        private static OrderTradeRequest TradeRequest(OrderTradeCommand command, TradeDto trade)
        {
            return new OrderTradeRequest("token", command, [trade], default(TransactionMode));
        }

        private static TradeDto Execution(int orderId, double quantity = 100)
        {
            return new TradeDto { OrderId = orderId, Way = OrderDtoWay.Buy, ExecutionQuantity = quantity, ExecutionAmountNet = quantity * 10 };
        }

        [Test]
        public void Should_add_a_trade_and_save_the_order()
        {
            var order = WorkingOrder(3);
            _orders.GetById(3).Returns(order);

            var results = _service.HandleRequest(TradeRequest(OrderTradeCommand.AddTrade, Execution(3, 100)));

            Assert.That(results.Single().Result.IsSuccess(), Is.True, results.Single().Result.ErrorMessage);
            Assert.That(order.OrderState, Is.EqualTo(OrderStateMachine.State.Validating));
            Assert.That(order.GetTrades().Single().ExecutionQuantity, Is.EqualTo(100));
            _orders.Received(1).Update(order);
        }

        [Test]
        public void Should_stay_in_dealing_after_a_partial_trade()
        {
            var order = WorkingOrder(3);
            _orders.GetById(3).Returns(order);

            _service.HandleRequest(TradeRequest(OrderTradeCommand.AddTrade, Execution(3, 40)));

            Assert.That(order.OrderState, Is.EqualTo(OrderStateMachine.State.Dealing));
            Assert.That(order.RemainingQuantity, Is.EqualTo(60));
        }

        [Test]
        public void Should_cancel_a_trade_by_its_id()
        {
            var order = WorkingOrder(3);
            _orders.GetById(3).Returns(order);
            _service.HandleRequest(TradeRequest(OrderTradeCommand.AddTrade, Execution(3, 40)));
            order.GetTrades().Single().Id = 7;

            var results = _service.HandleRequest(TradeRequest(OrderTradeCommand.CancelTrade, new TradeDto { OrderId = 3, Id = 7 }));

            Assert.That(results.Single().Result.IsSuccess(), Is.True, results.Single().Result.ErrorMessage);
            Assert.That(order.GetTrades().Single().TradeState, Is.EqualTo(TradeState.Cancelled));
            Assert.That(order.RemainingQuantity, Is.EqualTo(100));
        }

        [Test]
        public void Should_force_a_new_trade_to_booked_and_terminate_the_order_with_its_last_booked_trade()
        {
            var order = WorkingOrder(3);
            _orders.GetById(3).Returns(order);
            _service.HandleRequest(TradeRequest(OrderTradeCommand.AddTrade, Execution(3, 100)));
            order.GetTrades().Single().Id = 7;
            _service.HandleRequest(Request(OrderStateCommand.StartBooking, 3));

            var results = _service.HandleRequest(TradeRequest(OrderTradeCommand.BookTrade, new TradeDto { OrderId = 3, Id = 7 }));

            Assert.That(results.Single().Result.IsSuccess(), Is.True, results.Single().Result.ErrorMessage);
            Assert.That(order.GetTrades().Single().TradeState, Is.EqualTo(TradeState.Booked));
            Assert.That(order.OrderState, Is.EqualTo(OrderStateMachine.State.Terminated));
            Assert.That(order.OrderStatus, Is.EqualTo(OrderStatus.Filled));
        }

        [Test]
        public void Should_refuse_to_book_a_trade_before_the_booking_started_and_leave_the_order_unsaved()
        {
            var order = WorkingOrder(3);
            _orders.GetById(3).Returns(order);
            _service.HandleRequest(TradeRequest(OrderTradeCommand.AddTrade, Execution(3, 100)));
            order.GetTrades().Single().Id = 7;
            _orders.ClearReceivedCalls();

            var results = _service.HandleRequest(TradeRequest(OrderTradeCommand.BookTrade, new TradeDto { OrderId = 3, Id = 7 }));

            Assert.That(results.Single().Result.IsFailure(), Is.True);
            _orders.DidNotReceive().Update(Arg.Any<Order>());
        }

        [Test]
        public void Should_refuse_an_unknown_order_an_unknown_trade_an_empty_trade_and_an_unknown_user()
        {
            var order = WorkingOrder(3);
            _orders.GetById(3).Returns(order);

            Assert.That(_service.HandleRequest(TradeRequest(OrderTradeCommand.AddTrade, Execution(99))).Single().Result.ErrorMessage, Does.Contain("not found"));
            Assert.That(_service.HandleRequest(TradeRequest(OrderTradeCommand.CancelTrade, new TradeDto { OrderId = 3, Id = 5 })).Single().Result.ErrorMessage, Does.Contain("not found"));
            Assert.That(_service.HandleRequest(TradeRequest(OrderTradeCommand.AddTrade, Execution(3, 0))).Single().Result.IsFailure(), Is.True);
            var stranger = new OrderTradeRequest("nobody", OrderTradeCommand.AddTrade, [Execution(3)], default(TransactionMode));
            Assert.That(_service.HandleRequest(stranger).Single().Result.ErrorMessage, Is.EqualTo("Unknown user"));
            _orders.DidNotReceive().Update(Arg.Any<Order>());
        }
    }
}
