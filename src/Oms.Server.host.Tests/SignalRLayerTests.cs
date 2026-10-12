using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using Oms.Client.Services;
using Oms.Transport.Contracts;
using Oms.Transport.Contracts.Dto;
using Oms.Transport.Contracts.Messages;
using Oms.Transport.Contracts.Messages.Orders;

namespace Oms.Server.host.Tests
{
    /// <summary>
    /// The client's <see cref="OrderServiceProxy"/> against the real host (hub, JSON serialization, DI, a SQLite file) running
    /// in memory: what the unit tests below the transport cannot check.
    /// </summary>
    [TestFixture]
    public class SignalRLayerTests
    {
        private const string Login = "trader";
        private WebApplicationFactory<Program> _host;
        private OrderServiceProxy _proxy;
        private readonly ConcurrentQueue<Tuple<ItemOperationType, OrderDto[]>> _pushed = new ConcurrentQueue<Tuple<ItemOperationType, OrderDto[]>>();

        [OneTimeSetUp]
        public async Task StartTheHost()
        {
            _host = new WebApplicationFactory<Program>();
            var handler = _host.Server.CreateHandler();
            _proxy = new OrderServiceProxy("http://localhost", options =>
            {
                options.HttpMessageHandlerFactory = _ => handler;
                options.Transports = HttpTransportType.LongPolling;
            });
            _proxy.Subscribe(new Observer(_pushed.Enqueue));
            await _proxy.ConnectAsync();
        }

        [OneTimeTearDown]
        public async Task StopTheHost()
        {
            await _proxy.DisposeAsync();
            await _host.DisposeAsync();
        }

        [Test]
        public void Every_hub_method_the_client_calls_exists_on_the_hub()
        {
            var clientCalls = typeof(OrderHubMethods).GetFields(BindingFlags.Public | BindingFlags.Static)
                .Select(f => (string)f.GetRawConstantValue())
                .Except([OrderHubMethods.Path, OrderHubMethods.OrderListUpdated]);

            foreach (var name in clientCalls)
                Assert.That(typeof(OrderHub).GetMethod(name), Is.Not.Null, "OrderHub has no method " + name);
        }

        [Test]
        public async Task Should_answer_the_queries_with_the_demo_data()
        {
            Assert.That((await _proxy.GetSecuritiesAsync()).Count, Is.EqualTo(3));
            Assert.That((await _proxy.GetFundsAsync()).Select(f => f.Name), Is.EquivalentTo(["Alpha Fund", "Beta Fund"]));
            Assert.That(await _proxy.GetOrdersAsync(), Is.Not.Null);
        }

        [Test]
        public async Task Should_accept_a_new_order_and_cancel_it_in_two_steps_pushing_each_change()
        {
            var created = await CreateOrderAsync();
            Assert.That(created.Id, Is.GreaterThan(0));
            Assert.That(created.State, Is.EqualTo("Working"), "a sent order is accepted by the server");
            var add = await PushedAsync(ItemOperationType.Add, created.Id);
            Assert.That(add.Quantity, Is.EqualTo(100));
            Assert.That((await _proxy.GetOrdersAsync()).Select(o => o.Id), Does.Contain(created.Id));

            var cancel = await _proxy.SendAsync(new OrderRequest(Login, OrderStateCommand.Cancel, new List<int> { created.Id }, default(TransactionMode)));
            Assert.That(cancel.ResultList.Single().Item2.ErrorMessage, Is.Null);
            await PushedAsync(ItemOperationType.Update, created.Id, o => o.Status == "CancelPending");

            await _proxy.SendAsync(new OrderRequest(Login, OrderStateCommand.AcceptPending, new List<int> { created.Id }, default(TransactionMode)));
            await PushedAsync(ItemOperationType.Update, created.Id, o => o.State == "Terminated");
        }

        [Test]
        public async Task Should_stash_and_delete_a_draft_pushing_each_change()
        {
            var order = NewOrderDto(await _proxy.GetSecuritiesAsync(), await _proxy.GetFundsAsync());
            var stash = await _proxy.SendAsync(new OrderEditionRequest(Login, EditionCommand.Stash, new List<OrderDto> { order }, default(TransactionMode)));
            var id = stash.ResultList.Single().Item1.Id;
            Assert.That((await PushedAsync(ItemOperationType.Add, id)).State, Is.EqualTo("Draft"));

            await _proxy.SendAsync(new OrderRequest(Login, OrderStateCommand.Delete, new List<int> { id }, default(TransactionMode)));

            await PushedAsync(ItemOperationType.Delete, id);
        }

        [Test]
        public async Task Should_update_a_draft_order_once_the_pending_update_is_accepted()
        {
            var order = NewOrderDto(await _proxy.GetSecuritiesAsync(), await _proxy.GetFundsAsync());
            var stash = await _proxy.SendAsync(new OrderEditionRequest(Login, EditionCommand.Stash, [order], default(TransactionMode)));
            var draft = stash.ResultList.Single().Item1;
            draft.Quantity = 250;
            draft.PriceLimit = 9.5;

            var response = await _proxy.SendAsync(new OrderEditionRequest(Login, EditionCommand.Update, [draft], default(TransactionMode)));

            Assert.That(response.ResultList.Single().Item2.ErrorMessage, Is.Null);
            var pending = await PushedAsync(ItemOperationType.Update, draft.Id, o => o.Status == "UpdatePending");
            Assert.That(pending.Quantity, Is.EqualTo(100), "the order keeps its values until the update is accepted");

            var accept = await _proxy.SendAsync(new OrderRequest(Login, OrderStateCommand.AcceptPending, [draft.Id], default(TransactionMode)));
            Assert.That(accept.ResultList.Single().Item2.ErrorMessage, Is.Null);
            var updated = await PushedAsync(ItemOperationType.Update, draft.Id, o => o.Quantity == 250);
            Assert.That(updated.PriceLimit, Is.EqualTo(9.5));
            Assert.That(updated.Owner.Login, Is.EqualTo(Login));
        }

        [Test]
        public async Task Should_refuse_to_update_an_order_that_is_already_working()
        {
            var working = await CreateOrderAsync();
            working.Quantity = 250;

            var response = await _proxy.SendAsync(new OrderEditionRequest(Login, EditionCommand.Update, [working], default(TransactionMode)));

            Assert.That(response.ResultList.Single().Item2.ErrorMessage, Does.Contain("not allowed"));
        }

        [Test]
        public async Task Should_carry_the_refusals_of_every_command_family_back_to_the_client()
        {
            var id = (await CreateOrderAsync()).Id;

            var state = await _proxy.SendAsync(new OrderRequest(Login, OrderStateCommand.StartBooking, [id], default(TransactionMode)));
            var market = await _proxy.SendAsync(new OrderMarketRequest(Login, OrderMarketCommand.MarketCancel, [id], default(TransactionMode)));
            var trade = await _proxy.SendAsync(new OrderTradeRequest(Login, OrderTradeCommand.AddTrade,
                [new TradeDto { OrderId = id, ExecutionQuantity = 0 }], default(TransactionMode)));
            var unknown = await _proxy.SendAsync(new OrderRequest("nobody", OrderStateCommand.Cancel, [id], default(TransactionMode)));

            Assert.That(state.Command, Is.EqualTo(OrderStateCommand.StartBooking));
            Assert.That(state.ResultList.Single().Item2.ErrorMessage, Does.Contain("not allowed"));
            Assert.That(market.ResultList.Single().Item2.ErrorMessage, Is.Not.Null);
            Assert.That(trade.ResultList.Single().Item1, Is.EqualTo(id));
            Assert.That(trade.ResultList.Single().Item2.ErrorMessage, Is.Not.Null);
            Assert.That(unknown.ResultList.Single().Item2.ErrorMessage, Is.EqualTo("Unknown user"));
        }

        [Test]
        public async Task Should_execute_and_book_an_order_through_the_hub()
        {
            var id = (await CreateOrderAsync()).Id;
            var market = await _proxy.SendAsync(new OrderMarketRequest(Login, OrderMarketCommand.MarketSend, [id], default(TransactionMode)));
            Assert.That(market.ResultList.Single().Item2.ErrorMessage, Is.Null);
            await PushedAsync(ItemOperationType.Update, id, o => o.State == "Dealing");

            var added = await _proxy.SendAsync(new OrderTradeRequest(Login, OrderTradeCommand.AddTrade,
            [
                new TradeDto { OrderId = id, Way = OrderDtoWay.Buy, ExecutionQuantity = 100, ExecutionAmountNet = 1250 }
            ], default(TransactionMode)));
            Assert.That(added.ResultList.Single().Item2.ErrorMessage, Is.Null);
            var executed = await PushedAsync(ItemOperationType.Update, id, o => o.Trades.Count == 1);
            Assert.That(executed.State, Is.EqualTo("Validating"));

            await _proxy.SendAsync(new OrderRequest(Login, OrderStateCommand.StartBooking, [id], default(TransactionMode)));
            var booked = await _proxy.SendAsync(new OrderTradeRequest(Login, OrderTradeCommand.BookTrade,
                [new TradeDto { OrderId = id, Id = executed.Trades.Single().Id }], default(TransactionMode)));
            Assert.That(booked.ResultList.Single().Item2.ErrorMessage, Is.Null);

            var filled = (await _proxy.GetOrdersAsync()).Single(o => o.Id == id);
            Assert.That(filled.State, Is.EqualTo("Terminated"));
            Assert.That(filled.Status, Is.EqualTo("Filled"));
            Assert.That(filled.Trades.Single().State, Is.EqualTo("Booked"));
        }

        private static OrderDto NewOrderDto(List<SecurityDto> securities, List<FundDto> funds)
        {
            return new OrderDto
            {
                Quantity = 100,
                PriceLimit = 12.5,
                OrderType = OrderDtoType.Limit,
                Validity = OrderDtoValidityType.Day,
                Security = new SecurityDto { Id = securities.First().Id },
                Fund = new FundDto { Id = funds.First().Id },
                ClientOrderRef = "ref-" + Guid.NewGuid().ToString("N"),
            };
        }

        private async Task<OrderDto> CreateOrderAsync()
        {
            var order = NewOrderDto(await _proxy.GetSecuritiesAsync(), await _proxy.GetFundsAsync());
            var response = await _proxy.SendAsync(new OrderEditionRequest(Login, EditionCommand.Create, new List<OrderDto> { order }, default(TransactionMode)));
            Assert.That(response.ResultList.Single().Item2.ErrorMessage, Is.Null);
            return response.ResultList.Single().Item1;
        }

        /// <summary>Waits for the change the server pushed for the order.</summary>
        private async Task<OrderDto> PushedAsync(ItemOperationType type, int orderId, Func<OrderDto, bool> match = null)
        {
            for (var i = 0; i < 100; i++)
            {
                var found = _pushed.Where(p => p.Item1 == type).SelectMany(p => p.Item2).LastOrDefault(o => o.Id == orderId && (match == null || match(o)));
                if (found != null) return found;
                await Task.Delay(50);
            }
            throw new AssertionException("The server did not push " + type + " for order " + orderId);
        }

        private sealed class Observer : IObserver<Tuple<ItemOperationType, OrderDto[]>>
        {
            private readonly Action<Tuple<ItemOperationType, OrderDto[]>> _onNext;
            public Observer(Action<Tuple<ItemOperationType, OrderDto[]>> onNext) { _onNext = onNext; }
            public void OnNext(Tuple<ItemOperationType, OrderDto[]> value) => _onNext(value);
            public void OnError(Exception error) { }
            public void OnCompleted() { }
        }
    }
}
