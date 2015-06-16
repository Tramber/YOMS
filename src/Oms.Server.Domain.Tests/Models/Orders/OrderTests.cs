using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Oms.Server.Domain.Framework;
using Oms.Server.Domain.Models.EventLogs;
using Oms.Server.Domain.Models.Funds;
using Oms.Server.Domain.Models.Instruments;
using Oms.Server.Domain.Models.Orders;
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
            return new OrderBuilder()
                .WithCoreData(new User(), new User(), new OrderBasket(), "nnn", "truc")
                .WithFund(new Fund())
                .WithInstrument(new Instrument())
                .WithInitialReferentialData("instCode", InstrumentCodeType.Isin, "fundCode", FundCodeType.Other, "folioCode")
                .Build(isDraft).OrderState;
        }

        [TestCase(OrderStateMachine.State.Working, ExpectedResult = OrderStateMachine.State.Working)]
        public OrderStateMachine.State Should_stash_event_if_acceptance_is_required(OrderStateMachine.State initialState)
        {
            var order = new OrderBuilder()
                .WithCoreData(new User(), new User(), new OrderBasket(), "nnn", "truc")
                .WithFund(new Fund())
                .WithInstrument(new Instrument())
                .WithInitialReferentialData("instCode", InstrumentCodeType.Isin, "fundCode", FundCodeType.Other, "folioCode")
                .WithOrderState(initialState)
                .Build();

            var result = order.Cancel(new TriggerContext());

            Assert.That(result.IsSuccess(), Is.True, result.ErrorMessage);
            Assert.That(order.PendingTrigger, Is.Not.Null);
            return order.OrderState;
        }

         [TestCase(OrderStateMachine.State.Working, ExpectedResult = OrderStateMachine.State.Terminated)]
        public OrderStateMachine.State Should_activate_stash_event_when_this_event_is_accepted(OrderStateMachine.State initialState)
        {
            var order = new OrderBuilder()
                .WithCoreData(new User(), new User(), new OrderBasket(), "nnn", "truc")
                .WithFund(new Fund())
                .WithInstrument(new Instrument())
                .WithInitialReferentialData("instCode", InstrumentCodeType.Isin, "fundCode", FundCodeType.Other, "folioCode")
                .WithOrderState(initialState)
                .Build();

            var result = order.Cancel(new TriggerContext());
            var acceptResult = order.Cancel(new TriggerContext(), true);

            Assert.That(result.IsSuccess(), Is.True, result.ErrorMessage);
            Assert.That(acceptResult.IsSuccess(), result.ErrorMessage);
            Assert.That(order.PendingTrigger, Is.Null);
            return order.OrderState;
        }

    }
}
