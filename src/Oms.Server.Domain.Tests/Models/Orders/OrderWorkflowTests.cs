using System;
using System.Linq;
using NUnit.Framework;
using Oms.Server.Domain.Framework;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.EventLogs;
using Oms.Server.Domain.Models.Funds;
using Oms.Server.Domain.Models.Orders;
using Oms.Server.Domain.Models.Securities;
using Oms.Server.Domain.Models.Users;
using Oms.Server.Domain.Workflow;
using State = Oms.Server.Domain.Workflow.OrderStateMachine.State;

namespace Oms.Server.Domain.Tests.Models.Orders
{
    [TestFixture]
    public class OrderWorkflowTests
    {
        private static Order CreateOrder(State state)
        {
            return new OrderBuilderForTest()
                .WithOrderState(state)
                .WithCoreData(new User(), new User(), new OrderBasket(), "ref")
                .WithFund(new Fund())
                .WithSecurity(new Security())
                .WithInitialReferentialData("instCode", SecurityCodeType.Isin, "fundCode", FundCodeType.Other, "folioCode")
                .Build();
        }

        private static GenericResult Run(Order order, string command)
        {
            var context = new TriggerContext();
            switch (command)
            {
                case "SendRequest": return order.SendRequest(context);
                case "SendAccept": return order.SendAccept(context);
                case "SendReject": return order.SendReject(context);
                case "Recall": return order.Recall(context);
                case "Delete": return order.Delete(context);
                case "StartBooking": return order.StartBooking(context);
                case "MarketSend": return order.MarketSend(context, new OrderDealingEventParameter(null));
                case "Cancel": return order.Cancel(context);
                default: throw new ArgumentException(command);
            }
        }

        [TestCase(State.Undefined, "SendRequest", State.Accepting)]
        [TestCase(State.Draft, "SendRequest", State.Accepting)]
        [TestCase(State.Draft, "Delete", State.Terminated)]
        [TestCase(State.Accepting, "SendAccept", State.Working)]
        [TestCase(State.Accepting, "SendReject", State.Terminated)]
        [TestCase(State.Accepting, "Recall", State.Draft)]
        [TestCase(State.Working, "MarketSend", State.Dealing)]
        [TestCase(State.Dealing, "MarketSend", State.Dealing)]
        [TestCase(State.Validating, "StartBooking", State.Booking)]
        public void Should_move_to_the_next_state_and_log_one_event(State from, string command, State expected)
        {
            var order = CreateOrder(from);
            var eventCount = order.EventLogs.Count;

            var result = Run(order, command);

            Assert.That(result.IsSuccess(), Is.True, result.ErrorMessage);
            Assert.That(order.OrderState, Is.EqualTo(expected));
            Assert.That(order.EventLogs.Count, Is.EqualTo(eventCount + 1));
            Assert.That(order.EventLogs.Last().State, Is.EqualTo(expected));
        }

        [TestCase(State.Draft, "SendAccept")]
        [TestCase(State.Draft, "Cancel")]
        [TestCase(State.Working, "Recall")]
        [TestCase(State.Working, "Delete")]
        [TestCase(State.Dealing, "SendRequest")]
        [TestCase(State.Terminated, "Delete")]
        [TestCase(State.Terminated, "SendAccept")]
        public void Should_refuse_a_command_that_is_not_allowed_in_the_current_state(State from, string command)
        {
            var order = CreateOrder(from);
            var eventCount = order.EventLogs.Count;

            var result = Run(order, command);

            Assert.That(result.IsFailure(), Is.True);
            Assert.That(result.ErrorMessage, Does.Contain("not allowed"));
            Assert.That(order.OrderState, Is.EqualTo(from));
            Assert.That(order.EventLogs.Count, Is.EqualTo(eventCount));
        }

        [Test]
        public void Should_keep_the_state_while_a_cancel_is_pending()
        {
            var order = CreateOrder(State.Working);

            var result = order.Cancel(new TriggerContext());

            Assert.That(result.IsSuccess(), Is.True, result.ErrorMessage);
            Assert.That(order.OrderState, Is.EqualTo(State.Working));
            Assert.That(order.PendingTrigger, Is.EqualTo(OrderStateMachine.Trigger.Cancel));
            Assert.That(order.EventLogs.Last().TriggerStatus, Is.EqualTo(TriggerStatus.Pending));
        }

        [Test]
        public void Should_refuse_a_second_cancel_while_one_is_pending()
        {
            var order = CreateOrder(State.Working);
            order.Cancel(new TriggerContext());

            var result = order.Cancel(new TriggerContext());

            Assert.That(result.IsFailure(), Is.True);
            Assert.That(order.PendingTrigger, Is.EqualTo(OrderStateMachine.Trigger.Cancel));
        }

        [Test]
        public void Should_terminate_the_order_when_the_pending_cancel_is_accepted()
        {
            var order = CreateOrder(State.Working);
            order.Cancel(new TriggerContext());

            var result = order.AcceptPending(new TriggerContext());

            Assert.That(result.IsSuccess(), Is.True, result.ErrorMessage);
            Assert.That(order.OrderState, Is.EqualTo(State.Terminated));
            Assert.That(order.PendingTrigger, Is.Null);
            Assert.That(order.EventLogs.Last().TriggerStatus, Is.EqualTo(TriggerStatus.Accepted));
        }

        [Test]
        public void Should_keep_the_order_working_when_the_pending_cancel_is_rejected()
        {
            var order = CreateOrder(State.Working);
            order.Cancel(new TriggerContext());

            var result = order.RejectPending(new TriggerContext());

            Assert.That(result.IsSuccess(), Is.True, result.ErrorMessage);
            Assert.That(order.OrderState, Is.EqualTo(State.Working));
            Assert.That(order.PendingTrigger, Is.Null);
            Assert.That(order.EventLogs.Last().TriggerStatus, Is.EqualTo(TriggerStatus.Rejected));
        }

        [Test]
        public void Should_refuse_to_accept_or_reject_when_nothing_is_pending()
        {
            var order = CreateOrder(State.Working);

            Assert.That(order.AcceptPending(new TriggerContext()).IsFailure(), Is.True);
            Assert.That(order.RejectPending(new TriggerContext()).IsFailure(), Is.True);
            Assert.That(order.OrderState, Is.EqualTo(State.Working));
        }

        // Terminated is not covered: IsFilled() reports an order with no execution as filled (see AGENTS.md).
        [TestCase(State.Draft, OrderStatus.Pending)]
        [TestCase(State.Accepting, OrderStatus.New)]
        [TestCase(State.Working, OrderStatus.New)]
        public void Should_derive_the_order_status_from_the_state(State state, OrderStatus expected)
        {
            Assert.That(CreateOrder(state).OrderStatus, Is.EqualTo(expected));
        }

        [Test]
        public void Should_report_a_cancel_pending_status_while_a_cancel_is_pending()
        {
            var order = CreateOrder(State.Working);
            order.Cancel(new TriggerContext());

            Assert.That(order.OrderStatus, Is.EqualTo(OrderStatus.CancelPending));
        }
    }
}
