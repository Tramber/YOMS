using System.Linq;
using NUnit.Framework;
using Oms.Server.Domain.Framework;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.EventLogs;
using Oms.Server.Domain.Models.Funds;
using Oms.Server.Domain.Models.Orders;
using Oms.Server.Domain.Models.Securities;
using Oms.Server.Domain.Models.Trades;
using Oms.Server.Domain.Models.Users;
using Oms.Server.Domain.Workflow;

namespace Oms.Server.Domain.Tests.Models.Orders
{
    [TestFixture]
    public class OrderTests
    {
        [TestCase(true, ExpectedResult = OrderStateMachine.State.Draft)]
        [TestCase(false, ExpectedResult = OrderStateMachine.State.Accepting)]
        public OrderStateMachine.State Should_allow_create_a_new_order(bool isDraft)
        {
            return new OrderBuilderForTest()
                .WithOrderState(isDraft ? OrderStateMachine.State.Draft : OrderStateMachine.State.Accepting)
                .WithCoreData(new User(), new User(), new OrderBasket(), "nnn")
                .WithFund(new Fund())
                .WithSecurity(new Security())
                .WithInitialReferentialData("instCode", SecurityCodeType.Isin, "fundCode", FundCodeType.Other,"folioCode")
                .Build().OrderState;
        }

        [TestCase(OrderStateMachine.State.Working, ExpectedResult = OrderStateMachine.State.Working)]
        public OrderStateMachine.State Should_stash_event_if_acceptance_is_required(OrderStateMachine.State initialState)
        {
            var order = new OrderBuilderForTest()
                .WithOrderState(initialState)
                .WithCoreData(new User(), new User(), new OrderBasket(), "nnn")
                .WithFund(new Fund())
                .WithSecurity(new Security())
                .WithInitialReferentialData("instCode", SecurityCodeType.Isin, "fundCode", FundCodeType.Other, "folioCode")
                .Build();

            var result = order.Cancel(new TriggerContext());

            Assert.That(result.IsSuccess(), Is.True, result.ErrorMessage);
            Assert.That(order.PendingTrigger, Is.Not.Null);
            return order.OrderState;
        }

         [TestCase(OrderStateMachine.State.Working, ExpectedResult = OrderStateMachine.State.Terminated)]
        public OrderStateMachine.State Should_activate_stash_event_when_this_event_is_accepted(OrderStateMachine.State initialState)
        {
            var order = new OrderBuilderForTest()
                .WithOrderState(initialState)
                .WithCoreData(new User(), new User(), new OrderBasket(), "nnn")
                .WithFund(new Fund())
                .WithSecurity(new Security())
                .WithInitialReferentialData("instCode", SecurityCodeType.Isin, "fundCode", FundCodeType.Other, "folioCode")
                .Build();

            var result = order.Cancel(new TriggerContext());
            var acceptResult = order.AcceptPending(new TriggerContext());

            Assert.That(result.IsSuccess(), Is.True, result.ErrorMessage);
            Assert.That(acceptResult.IsSuccess(), result.ErrorMessage);
            Assert.That(order.PendingTrigger, Is.Null);
            return order.OrderState;
        }

        [Test]
        public void Should_cancel_a_trade_when_the_order_is_dealing()
        {
            var order = CreateOrder(OrderStateMachine.State.Dealing);
            var trade = new Trade();
            trade.Create(new TriggerContext(), new TradeEditableEventParameter { ExecutionQuantity = 100 });

            var result = order.CancelTrade(new TriggerContext(), trade);

            Assert.That(result.IsSuccess(), Is.True, result.ErrorMessage);
            Assert.That(order.OrderState, Is.EqualTo(OrderStateMachine.State.Dealing));
            var lastEvent = order.EventLogs.Last();
            Assert.That(lastEvent.Trigger, Is.EqualTo(OrderStateMachine.Trigger.CancelTrade));
            Assert.That(((OrderParameterEventLog<IOrderDealingData>)lastEvent).Parameters.Trade, Is.SameAs(trade));
        }

        [Test]
        public void Should_copy_the_core_data_onto_the_order_when_it_is_sent()
        {
            var creator = new User();
            var owner = new User();
            var basket = new OrderBasket();
            var builder = new OrderBuilder()
                .WithCoreData(creator, owner, basket, "client-ref")
                .WithFund(new Fund())
                .WithSecurity(new Security())
                .WithPrices(OrderType.Limit, 100, 12.5, null)
                .WithInitialReferentialData("instCode", SecurityCodeType.Isin, "fundCode", FundCodeType.Other, "folioCode");
            var coreData = builder.BuildOrderCoreData();
            coreData.IsAutoAccepted = true;
            var order = new Order();

            var result = order.SendRequest(new TriggerContext(creator), coreData, builder.BuildOrderEditableData());

            Assert.That(result.IsSuccess(), Is.True, result.ErrorMessage);
            Assert.That(order.Creator, Is.SameAs(creator));
            Assert.That(order.Owner, Is.SameAs(owner));
            Assert.That(order.OrderBasket, Is.SameAs(basket));
            Assert.That(order.ClientOrderRef, Is.EqualTo("client-ref"));
            Assert.That(order.IsAutoAccepted, Is.True);
            Assert.That(order.InitialReferentialData, Is.SameAs(coreData.InitialReferentialData));
            Assert.That(order.OrderState, Is.EqualTo(OrderStateMachine.State.Accepting));
        }

        [Test]
        public void Should_leave_the_order_unchanged_when_there_is_no_core_data()
        {
            var order = CreateOrder(OrderStateMachine.State.Draft);
            var reference = order.ClientOrderRef;

            Assert.That(order.InsertDataFrom(null), Is.SameAs(order));
            Assert.That(order.ClientOrderRef, Is.EqualTo(reference));
        }

        [Test]
        public void Should_refuse_to_cancel_a_trade_when_the_order_is_working()
        {
            var order = CreateOrder(OrderStateMachine.State.Working);
            var eventCount = order.EventLogs.Count;

            var result = order.CancelTrade(new TriggerContext(), new Trade());

            Assert.That(result.IsFailure(), Is.True);
            Assert.That(order.OrderState, Is.EqualTo(OrderStateMachine.State.Working));
            Assert.That(order.EventLogs.Count, Is.EqualTo(eventCount));
        }

        private static Order CreateOrder(OrderStateMachine.State state)
        {
            return new OrderBuilderForTest()
                .WithOrderState(state)
                .WithCoreData(new User(), new User(), new OrderBasket(), "nnn")
                .WithFund(new Fund())
                .WithSecurity(new Security())
                .WithInitialReferentialData("instCode", SecurityCodeType.Isin, "fundCode", FundCodeType.Other, "folioCode")
                .Build();
        }
    }
}
