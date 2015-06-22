using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Oms.Server.Domain.Framework;
using Oms.Server.Domain.Models.EventLogs;
using Oms.Server.Domain.Models.Funds;
using Oms.Server.Domain.Models.Orders;
using Oms.Server.Domain.Models.Securities;
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
                .WithOrderState(OrderStateMachine.State.Draft)
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

    }
}
