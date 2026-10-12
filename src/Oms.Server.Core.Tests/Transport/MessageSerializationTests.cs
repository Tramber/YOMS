using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using NUnit.Framework;
using Oms.Transport.Contracts.Dto;
using Oms.Transport.Contracts.Messages;
using Oms.Transport.Contracts.Messages.Orders;

namespace Oms.Server.Core.Tests.Transport
{
    /// <summary>The hub serialises messages with System.Text.Json (web defaults): they must survive the trip to the client.</summary>
    [TestFixture]
    public class MessageSerializationTests
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        private static T RoundTrip<T>(T value)
        {
            return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value, Options), Options);
        }

        [Test]
        public void A_request_keeps_its_token_command_and_orders()
        {
            var request = new OrderRequest("trader", OrderStateCommand.Cancel, [3, 4], default(TransactionMode));

            Assert.That(request.Token, Is.EqualTo("trader"));
            var copy = JsonSerializer.Deserialize<OrderRequest>(JsonSerializer.Serialize(request, Options), Options);

            Assert.That(copy.Token, Is.EqualTo("trader"));
            Assert.That(copy.Command, Is.EqualTo(OrderStateCommand.Cancel));
            Assert.That(copy.ItemList, Is.EqualTo([3, 4]));
        }

        [Test]
        public void A_market_request_carries_a_market_command()
        {
            var request = new OrderMarketRequest("trader", OrderMarketCommand.MarketCancel, [3], default(TransactionMode));

            var copy = JsonSerializer.Deserialize<OrderMarketRequest>(JsonSerializer.Serialize(request, Options), Options);

            Assert.That(copy.Command, Is.EqualTo(OrderMarketCommand.MarketCancel));
        }

        [Test]
        public void A_state_response_keeps_the_outcome_of_each_order_including_the_error()
        {
            var response = new Response<Tuple<int, GenericResultDto>, OrderStateCommand>(OrderStateCommand.Cancel, [
                Tuple.Create(1, new GenericResultDto(null, 0)),
                Tuple.Create(2, new GenericResultDto("refused", 1))
            ]);

            var copy = RoundTrip(response);

            Assert.That(copy.Command, Is.EqualTo(OrderStateCommand.Cancel));
            Assert.That(copy.ResultList.Select(r => r.Item1), Is.EqualTo([1, 2]));
            Assert.That(copy.ResultList[1].Item2.ErrorMessage, Is.EqualTo("refused"));
            Assert.That(copy.ResultList[1].Item2.ErrorCode, Is.EqualTo(1));
        }

        [Test]
        public void An_edition_response_keeps_the_orders_and_their_outcome()
        {
            var response = new Response<Tuple<OrderDto, GenericResultDto>, EditionCommand>(EditionCommand.Create, [
                Tuple.Create(
                    new OrderDto
                    {
                        Id = 9, Quantity = 50, OrderType = OrderDtoType.Limit, Owner = new UserDto { Login = "trader" }
                    }, new GenericResultDto("bad", 1))
            ]);

            var copy = RoundTrip(response);

            Assert.That(copy.ResultList.Single().Item1.Id, Is.EqualTo(9));
            Assert.That(copy.ResultList.Single().Item1.OrderType, Is.EqualTo(OrderDtoType.Limit));
            Assert.That(copy.ResultList.Single().Item1.Owner.Login, Is.EqualTo("trader"));
            Assert.That(copy.ResultList.Single().Item2.ErrorMessage, Is.EqualTo("bad"));
        }

        [Test]
        public void A_trade_request_and_the_trades_of_an_order_survive_json()
        {
            var request = new OrderTradeRequest("trader", OrderTradeCommand.AddTrade, [
                new TradeDto { OrderId = 4, Way = OrderDtoWay.Sell, ExecutionQuantity = 10, ExecutionAmountNet = 125.5 }
            ], default(TransactionMode));
            var order = new OrderDto { Id = 4, Trades = [new TradeDto { Id = 9, OrderId = 4, State = "Booked" }] };

            var requestCopy = JsonSerializer.Deserialize<OrderTradeRequest>(JsonSerializer.Serialize(request, Options), Options);
            var orderCopy = JsonSerializer.Deserialize<OrderDto>(JsonSerializer.Serialize(order, Options), Options);

            Assert.That(requestCopy.Command, Is.EqualTo(OrderTradeCommand.AddTrade));
            Assert.That(requestCopy.ItemList.Single().Way, Is.EqualTo(OrderDtoWay.Sell));
            Assert.That(requestCopy.ItemList.Single().ExecutionAmountNet, Is.EqualTo(125.5));
            Assert.That(orderCopy.Trades.Single().State, Is.EqualTo("Booked"));
        }
    }
}
