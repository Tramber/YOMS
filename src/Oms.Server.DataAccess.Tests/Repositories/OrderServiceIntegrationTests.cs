using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Oms.Server.Core.Contracts;
using Oms.Server.Core.Services;
using Oms.Server.DataAccess.NHibernate.Repositories;
using Oms.Server.Domain.Models.Funds;
using Oms.Server.Domain.Models.Securities;
using Oms.Server.Domain.Models.Users;
using Oms.Server.Domain.Workflow;
using Oms.Transport.Contracts;
using Oms.Transport.Contracts.Dto;
using Oms.Transport.Contracts.Messages;
using Oms.Transport.Contracts.Messages.Orders;

namespace Oms.Server.DataAccess.Tests.Repositories
{
    /// <summary>The order service from the contract down to a real SQLite database.</summary>
    [TestFixture]
    public class OrderServiceIntegrationTests
    {
        private RepositoryFacade _repositories;
        private OrderServiceContract _contract;
        private List<Tuple<ItemOperationType, OrderDto[]>> _published;
        private string _login;
        private Security _security;
        private Fund _fund;

        [SetUp]
        public void SetUp()
        {
            var suffix = Guid.NewGuid().ToString("N");
            _login = "trader-" + suffix;
            var user = new User { Login = _login, IsActive = true };
            _fund = new Fund { Name = "Fund " + suffix };
            _security = new Security { Name = "Security " + suffix, Creator = user, BloombergTicker = "T", CreationDate = DateTime.Now, Currency = Currency.Eur, Type = SecurityType.Equity, IsActive = true };
            _repositories = new RepositoryFacade();
            _repositories.Users.Add(user);
            _repositories.Funds.Add(_fund);
            _repositories.Securities.Add(_security);
            _contract = new OrderServiceContract(new OrderManagementService(_repositories));
            _published = [];
            _contract.OrderListUpdated += (type, orders) => _published.Add(Tuple.Create(type, orders));
        }

        private OrderDto NewOrder()
        {
            return new OrderDto
            {
                Quantity = 100,
                PriceLimit = 12.5,
                OrderType = OrderDtoType.Limit,
                Validity = OrderDtoValidityType.Day,
                Security = new SecurityDto { Id = _security.Id },
                Fund = new FundDto { Id = _fund.Id },
                ClientOrderRef = "ref-" + Guid.NewGuid().ToString("N"),
            };
        }

        private int CreateOrder()
        {
            var response = _contract.Handle(new OrderEditionRequest(_login, EditionCommand.Create, [NewOrder()], default(TransactionMode)));
            Assert.That(response.ResultList.Single().Item2.ErrorMessage, Is.Null);
            return response.ResultList.Single().Item1.Id;
        }

        private int CreateWorkingOrder()
        {
            var id = CreateOrder();
            var order = _repositories.Orders.GetOrderById(id);
            order.SendAccept(new Oms.Server.Domain.Models.EventLogs.TriggerContext(_repositories.Users.GetUserByLogin(_login)));
            _repositories.Orders.Update(order);
            return id;
        }

        private GenericResultDto SendTrade(OrderTradeCommand command, TradeDto trade)
        {
            return _contract.Handle(new OrderTradeRequest(_login, command, [trade], default(TransactionMode))).ResultList.Single().Item2;
        }

        [Test]
        public void Should_execute_and_book_an_order_across_requests()
        {
            var id = CreateWorkingOrder();

            var added = SendTrade(OrderTradeCommand.AddTrade, new TradeDto { OrderId = id, Way = OrderDtoWay.Buy, ExecutionQuantity = 100, ExecutionAmountNet = 1250 });
            Assert.That(added.ErrorMessage, Is.Null);
            var executed = _repositories.Orders.GetOrderById(id);
            Assert.That(executed.OrderState, Is.EqualTo(OrderStateMachine.State.Validating));
            var trade = _published.Last().Item2.Single().Trades.Single();
            Assert.That(trade.Id, Is.GreaterThan(0));
            Assert.That(trade.State, Is.EqualTo("New"));

            _contract.Handle(new OrderRequest(_login, OrderStateCommand.StartBooking, [id], default(TransactionMode)));
            var booked = SendTrade(OrderTradeCommand.BookTrade, new TradeDto { OrderId = id, Id = trade.Id });

            Assert.That(booked.ErrorMessage, Is.Null);
            var done = _repositories.Orders.GetOrderById(id);
            Assert.That(done.OrderState, Is.EqualTo(OrderStateMachine.State.Terminated));
            Assert.That(done.OrderStatus.ToString(), Is.EqualTo("Filled"));
            Assert.That(_published.Last().Item2.Single().Trades.Single().State, Is.EqualTo("Booked"));
        }

        [Test]
        public void Should_cancel_a_trade_and_put_the_order_back_to_dealing()
        {
            var id = CreateWorkingOrder();
            SendTrade(OrderTradeCommand.AddTrade, new TradeDto { OrderId = id, Way = OrderDtoWay.Buy, ExecutionQuantity = 100, ExecutionAmountNet = 1250 });
            var trade = _published.Last().Item2.Single().Trades.Single();

            var cancelled = SendTrade(OrderTradeCommand.CancelTrade, new TradeDto { OrderId = id, Id = trade.Id });

            Assert.That(cancelled.ErrorMessage, Is.Null);
            var order = _repositories.Orders.GetOrderById(id);
            Assert.That(order.OrderState, Is.EqualTo(OrderStateMachine.State.Dealing));
            Assert.That(order.RemainingQuantity, Is.EqualTo(100));
            Assert.That(_published.Last().Item2.Single().Trades.Single().State, Is.EqualTo("Cancelled"));
        }

        [Test]
        public void Should_refuse_an_unknown_trade_and_a_trade_on_a_draft_order()
        {
            var id = CreateWorkingOrder();

            Assert.That(SendTrade(OrderTradeCommand.BookTrade, new TradeDto { OrderId = id, Id = 999999 }).ErrorMessage, Does.Contain("not found"));
            Assert.That(SendTrade(OrderTradeCommand.AddTrade, new TradeDto { OrderId = id, ExecutionQuantity = 0 }).ErrorMessage, Does.Contain("positive"));
            Assert.That(SendTrade(OrderTradeCommand.AddTrade, new TradeDto { OrderId = 999999, ExecutionQuantity = 1 }).ErrorMessage, Does.Contain("not found"));
        }

        [Test]
        public void Should_create_an_order_that_is_saved_and_published()
        {
            var response = _contract.Handle(new OrderEditionRequest(_login, EditionCommand.Create, [NewOrder()], default(TransactionMode)));

            var created = response.ResultList.Single().Item1;
            Assert.That(response.ResultList.Single().Item2.ErrorMessage, Is.Null);
            Assert.That(created.Id, Is.GreaterThan(0));
            Assert.That(created.Quantity, Is.EqualTo(100));
            Assert.That(created.Security.Id, Is.EqualTo(_security.Id));
            Assert.That(created.Creator.Login, Is.EqualTo(_login));

            var saved = _repositories.Orders.GetOrderById(created.Id);
            Assert.That(saved.OrderState, Is.EqualTo(OrderStateMachine.State.Accepting));
            Assert.That(saved.ClientOrderRef, Is.EqualTo(created.ClientOrderRef));
            Assert.That(_published.Single().Item1, Is.EqualTo(ItemOperationType.Add));
            Assert.That(_published.Single().Item2.Single().Id, Is.EqualTo(created.Id));
        }

        [Test]
        public void Should_recall_then_delete_a_draft_across_requests()
        {
            var id = CreateOrder();
            _contract.Handle(new OrderRequest(_login, OrderStateCommand.Recall, [id], default(TransactionMode)));
            _contract.Handle(new OrderRequest(_login, OrderStateCommand.Delete, [id], default(TransactionMode)));

            Assert.That(_repositories.Orders.GetOrderById(id).OrderState, Is.EqualTo(OrderStateMachine.State.Terminated));
        }

        [Test]
        public void Should_cancel_a_working_order_in_two_steps_across_requests()
        {
            var id = CreateOrder();
            var order = _repositories.Orders.GetOrderById(id);
            order.SendAccept(new Oms.Server.Domain.Models.EventLogs.TriggerContext(_repositories.Users.GetUserByLogin(_login)));
            _repositories.Orders.Update(order);

            var cancel = _contract.Handle(new OrderRequest(_login, OrderStateCommand.Cancel, [id], default(TransactionMode)));
            Assert.That(cancel.ResultList.Single().Item2.ErrorMessage, Is.Null);
            var pending = _repositories.Orders.GetOrderById(id);
            Assert.That(pending.OrderState, Is.EqualTo(OrderStateMachine.State.Working));
            Assert.That(pending.PendingTrigger, Is.EqualTo(OrderStateMachine.Trigger.Cancel));

            var accept = _contract.Handle(new OrderRequest(_login, OrderStateCommand.AcceptPending, [id], default(TransactionMode)));
            Assert.That(accept.ResultList.Single().Item2.ErrorMessage, Is.Null);
            var cancelled = _repositories.Orders.GetOrderById(id);
            Assert.That(cancelled.OrderState, Is.EqualTo(OrderStateMachine.State.Terminated));
            Assert.That(cancelled.PendingTrigger, Is.Null);
            Assert.That(_published.Select(p => p.Item1), Is.EqualTo([ItemOperationType.Add, ItemOperationType.Update, ItemOperationType.Update
            ]));
        }

        [Test]
        public void Should_answer_a_refused_command_without_saving_it()
        {
            var id = CreateOrder();
            _published.Clear();

            var response = _contract.Handle(new OrderRequest(_login, OrderStateCommand.StartBooking, [id], default(TransactionMode)));

            Assert.That(response.ResultList.Single().Item2.ErrorMessage, Does.Contain("not allowed"));
            Assert.That(_repositories.Orders.GetOrderById(id).EventLogs.Count, Is.EqualTo(1));
            Assert.That(_published, Is.Empty);
        }

        [Test]
        public void Should_list_the_orders_securities_and_funds_that_were_saved()
        {
            var id = CreateOrder();

            var order = _contract.GetOrders().Single(o => o.Id == id);

            Assert.That(order.State, Is.EqualTo("Accepting"));
            Assert.That(order.Status, Is.EqualTo("New"));
            Assert.That(order.RemainingQuantity, Is.EqualTo(100));
            Assert.That(order.Security.Id, Is.EqualTo(_security.Id));
            Assert.That(_contract.GetSecurities().Select(sec => sec.Id), Does.Contain(_security.Id));
            Assert.That(_contract.GetFunds().Select(f => f.Id), Does.Contain(_fund.Id));
        }

        [Test]
        public void Should_keep_the_side_of_a_sell_order()
        {
            var dto = NewOrder();
            dto.Way = OrderDtoWay.Sell;

            var response = _contract.Handle(new OrderEditionRequest(_login, EditionCommand.Create, [dto], default(TransactionMode)));

            Assert.That(response.ResultList.Single().Item2.ErrorMessage, Is.Null);
            Assert.That(response.ResultList.Single().Item1.Way, Is.EqualTo(OrderDtoWay.Sell));
            Assert.That(_contract.GetOrders().Single(o => o.Id == response.ResultList.Single().Item1.Id).Way, Is.EqualTo(OrderDtoWay.Sell));
        }

        [Test]
        public void Should_refuse_an_unknown_user()
        {
            var response = _contract.Handle(new OrderEditionRequest("nobody", EditionCommand.Create, [NewOrder()], default(TransactionMode)));

            Assert.That(response.ResultList.Single().Item2.ErrorMessage, Does.Contain("Unknown user"));
            Assert.That(_published, Is.Empty);
        }
    }
}
