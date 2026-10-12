using System.Linq;
using NUnit.Framework;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Framework;
using Oms.Server.Domain.Models.EventLogs;
using Oms.Server.Domain.Models.Funds;
using Oms.Server.Domain.Models.Orders;
using Oms.Server.Domain.Models.Securities;
using Oms.Server.Domain.Models.Trades;
using Oms.Server.Domain.Models.Users;
using Oms.Server.Domain.Workflow;
using State = Oms.Server.Domain.Workflow.OrderStateMachine.State;

namespace Oms.Server.Domain.Tests.Models.Orders
{
    [TestFixture]
    public class OrderQuantityTests
    {
        private static Trade CreateTrade(double quantity, bool cancelled = false)
        {
            var trade = new Trade();
            trade.Create(new TriggerContext(), new TradeEditableEventParameter { ExecutionQuantity = quantity });
            if (cancelled)
                trade.Cancel(new TriggerContext());
            return trade;
        }

        private static Order CreateOrder(State state, double quantity, params Trade[] trades)
        {
            var builder = new OrderBuilderForTest().WithOrderState(state);
            foreach (var trade in trades)
                builder.WithTrade(trade);
            return builder
                .WithCoreData(new User(), new User(), new OrderBasket(), "ref")
                .WithFund(new Fund())
                .WithSecurity(new Security())
                .WithPrices(OrderType.Limit, quantity, 10, null)
                .WithInitialReferentialData("instCode", SecurityCodeType.Isin, "fundCode", FundCodeType.Other, "folioCode")
                .Build();
        }

        [Test]
        public void Should_have_the_whole_quantity_remaining_before_any_execution()
        {
            var order = CreateOrder(State.Working, 100);

            Assert.That(order.RemainingQuantity, Is.EqualTo(100));
            Assert.That(order.OrderStatus, Is.EqualTo(OrderStatus.New));
        }

        [Test]
        public void Should_remove_the_executed_trades_from_the_remaining_quantity()
        {
            var order = CreateOrder(State.Dealing, 100, CreateTrade(30), CreateTrade(20));

            Assert.That(order.RemainingQuantity, Is.EqualTo(50));
            Assert.That(order.OrderStatus, Is.EqualTo(OrderStatus.PartiallyFilled));
        }

        [Test]
        public void Should_ignore_a_cancelled_trade()
        {
            var order = CreateOrder(State.Dealing, 100, CreateTrade(30, cancelled: true));

            Assert.That(order.RemainingQuantity, Is.EqualTo(100));
        }

        [TestCase(100, OrderStatus.Filled)]
        [TestCase(60, OrderStatus.PartiallyFilled)]
        [TestCase(130, OrderStatus.OverFilled)]
        public void Should_derive_the_status_of_a_validating_order_from_what_was_executed(double executed, OrderStatus expected)
        {
            var order = CreateOrder(State.Validating, 100, CreateTrade(executed));

            Assert.That(order.OrderStatus, Is.EqualTo(expected));
        }

        [Test]
        public void Should_report_a_cancelled_order_with_no_execution_as_cancelled()
        {
            Assert.That(CreateOrder(State.Terminated, 100).OrderStatus, Is.EqualTo(OrderStatus.Cancelled));
        }

        [Test]
        public void Should_report_a_cancelled_fully_executed_order_as_cancelled_filled()
        {
            Assert.That(CreateOrder(State.Terminated, 100, CreateTrade(100)).OrderStatus, Is.EqualTo(OrderStatus.CancelledFilled));
        }

        [Test]
        public void Should_not_consider_an_order_without_quantity_as_filled()
        {
            var order = CreateOrder(State.Terminated, 0);

            Assert.That(order.OrderStatus, Is.EqualTo(OrderStatus.Cancelled));
        }

        [Test]
        public void Should_update_the_remaining_quantity_when_a_trade_is_sent_to_the_market()
        {
            var order = CreateOrder(State.Working, 100);

            Assert.That(order.MarketSend(new TriggerContext(), new OrderDealingEventParameter(CreateTrade(40))).IsSuccess(), Is.True);
            Assert.That(order.RemainingQuantity, Is.EqualTo(60));

            Assert.That(order.MarketSend(new TriggerContext(), new OrderDealingEventParameter(CreateTrade(60))).IsSuccess(), Is.True);
            Assert.That(order.RemainingQuantity, Is.EqualTo(0));
        }

        [Test]
        public void Should_rebuild_the_remaining_quantity_from_the_event_log()
        {
            var order = CreateOrder(State.Working, 100);
            order.MarketSend(new TriggerContext(new User(), System.DateTime.Now.AddMinutes(10)), new OrderDealingEventParameter(CreateTrade(40)));

            var rebuilt = new Order { EventLogs = order.EventLogs.ToList() };

            Assert.That(rebuilt.RemainingQuantity, Is.EqualTo(60));
        }
    }
}
