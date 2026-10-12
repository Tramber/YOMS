using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using NUnit.Framework;
using Oms.Client.Services;
using Oms.Transport.Contracts;
using Oms.Transport.Contracts.Dto;
using Oms.Transport.Contracts.Messages;
using Oms.Transport.Contracts.Messages.Orders;

namespace Oms.Server.host.Tests
{
    /// <summary>
    /// The same host on a real loopback port (Kestrel) with the transport the client really uses, WebSockets, which the
    /// in-memory tests cannot use.
    /// </summary>
    [TestFixture]
    public class LiveSignalRTests
    {
        private WebApplicationFactory<Program> _host;
        private string _url;

        [OneTimeSetUp]
        public void StartTheHost()
        {
            _host = new WebApplicationFactory<Program>();
            _host.UseKestrel();
            _host.StartServer();
            _url = _host.ClientOptions.BaseAddress.ToString().TrimEnd('/');
        }

        [OneTimeTearDown]
        public async Task StopTheHost()
        {
            await _host.DisposeAsync();
        }

        private static OrderEditionRequest NewOrder(SecurityDto security, FundDto fund)
        {
            var order = new OrderDto
            {
                Quantity = 10,
                PriceLimit = 1,
                OrderType = OrderDtoType.Limit,
                Validity = OrderDtoValidityType.Day,
                Security = security,
                Fund = fund,
                ClientOrderRef = "live-" + Guid.NewGuid().ToString("N"),
            };
            return new OrderEditionRequest("trader", EditionCommand.Create, new List<OrderDto> { order }, default(TransactionMode));
        }

        [Test]
        public async Task Should_serve_the_client_over_web_sockets_and_push_the_changes()
        {
            var pushed = new List<OrderDto>();
            await using var proxy = new OrderServiceProxy(_url);
            proxy.Subscribe(new Observer(change => { lock (pushed) pushed.AddRange(change.Item2); }));
            await proxy.ConnectAsync();
            Assert.That(proxy.IsConnected, Is.True);

            var response = await proxy.SendAsync(NewOrder((await proxy.GetSecuritiesAsync()).First(), (await proxy.GetFundsAsync()).First()));

            var id = response.ResultList.Single().Item1.Id;
            Assert.That(response.ResultList.Single().Item2.ErrorMessage, Is.Null);
            await WaitAsync(() => { lock (pushed) return pushed.Any(o => o.Id == id); });
        }

        [Test]
        public async Task Should_stop_pushing_to_a_client_that_unsubscribed()
        {
            var received = 0;
            await using var listener = new HubConnectionBuilder().WithUrl(_url + OrderHubMethods.Path, Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets).Build();
            listener.On<ItemOperationType, OrderDto[]>(OrderHubMethods.OrderListUpdated, (type, orders) => received++);
            await listener.StartAsync();
            await listener.InvokeAsync(OrderHubMethods.Subscribe);
            await using var sender = new OrderServiceProxy(_url);
            await sender.ConnectAsync();
            var request = NewOrder((await sender.GetSecuritiesAsync()).First(), (await sender.GetFundsAsync()).First());

            await sender.SendAsync(request);
            await WaitAsync(() => received == 1);
            await listener.InvokeAsync(OrderHubMethods.Unsubscribe);
            await sender.SendAsync(NewOrder(request.ItemList[0].Security, request.ItemList[0].Fund));
            await Task.Delay(500);

            Assert.That(received, Is.EqualTo(1));
        }

        private static async Task WaitAsync(Func<bool> condition)
        {
            for (var i = 0; i < 100 && !condition(); i++)
                await Task.Delay(50);
            Assert.That(condition(), Is.True, "The server did not push the change");
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
