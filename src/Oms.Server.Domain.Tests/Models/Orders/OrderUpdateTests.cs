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
    public class OrderUpdateTests
    {
        private static Order CreateOrder(State state, double quantity)
        {
            return new OrderBuilderForTest()
                .WithOrderState(state)
                .WithCoreData(new User(), new User(), new OrderBasket(), "ref")
                .WithFund(new Fund())
                .WithSecurity(new Security())
                .WithPrices(OrderType.Limit, quantity, 10, null)
                .WithInitialReferentialData("instCode", SecurityCodeType.Isin, "fundCode", FundCodeType.Other, "folioCode")
                .Build();
        }

        private static OrderBuilder BuilderFor(double quantity)
        {
            return new OrderBuilder()
                .WithCoreData(new User(), new User(), new OrderBasket(), "new-ref")
                .WithFund(new Fund())
                .WithSecurity(new Security())
                .WithPrices(OrderType.Limit, quantity, 10, null)
                .WithInitialReferentialData("instCode", SecurityCodeType.Isin, "fundCode", FundCodeType.Other, "folioCode");
        }

        private static GenericResult Update(Order order, double quantity, DateTime? date = null)
        {
            var builder = BuilderFor(quantity);
            return order.Update(new TriggerContext(new User(), date), builder.BuildOrderCoreData(), builder.BuildOrderEditableData());
        }

        [Test]
        public void Should_keep_the_current_data_while_an_update_is_pending()
        {
            var order = CreateOrder(State.Draft, 100);

            var result = Update(order, 200);

            Assert.That(result.IsSuccess(), Is.True, result.ErrorMessage);
            Assert.That(order.Quantity, Is.EqualTo(100));
            Assert.That(order.RemainingQuantity, Is.EqualTo(100));
            Assert.That(order.PendingTrigger, Is.EqualTo(OrderStateMachine.Trigger.Update));
            Assert.That(order.OrderStatus, Is.EqualTo(OrderStatus.UpdatePending));
            Assert.That(order.OrderState, Is.EqualTo(State.Draft));
            Assert.That(order.EventLogs.Last().TriggerStatus, Is.EqualTo(TriggerStatus.Pending));
        }

        [Test]
        public void Should_not_change_the_core_data_while_an_update_is_pending()
        {
            var order = CreateOrder(State.Draft, 100);
            var reference = order.ClientOrderRef;

            Update(order, 200);

            Assert.That(order.ClientOrderRef, Is.EqualTo(reference));
        }

        [Test]
        public void Should_apply_the_data_sent_with_the_update_when_it_is_accepted()
        {
            var order = CreateOrder(State.Draft, 100);
            Update(order, 200);

            var result = order.AcceptPending(new TriggerContext());

            Assert.That(result.IsSuccess(), Is.True, result.ErrorMessage);
            Assert.That(order.Quantity, Is.EqualTo(200));
            Assert.That(order.RemainingQuantity, Is.EqualTo(200));
            Assert.That(order.PendingTrigger, Is.Null);
            Assert.That(order.OrderState, Is.EqualTo(State.Draft));
            Assert.That(order.EventLogs.Last().TriggerStatus, Is.EqualTo(TriggerStatus.Accepted));
        }

        [Test]
        public void Should_discard_the_data_sent_with_the_update_when_it_is_rejected()
        {
            var order = CreateOrder(State.Draft, 100);
            Update(order, 200);

            var result = order.RejectPending(new TriggerContext());

            Assert.That(result.IsSuccess(), Is.True, result.ErrorMessage);
            Assert.That(order.Quantity, Is.EqualTo(100));
            Assert.That(order.PendingTrigger, Is.Null);
            Assert.That(order.OrderStatus, Is.EqualTo(OrderStatus.Pending));
        }

        [Test]
        public void Should_move_an_order_in_acceptance_to_working_when_its_update_is_accepted()
        {
            var order = CreateOrder(State.Accepting, 100);
            Update(order, 150);

            order.AcceptPending(new TriggerContext());

            Assert.That(order.OrderState, Is.EqualTo(State.Working));
            Assert.That(order.Quantity, Is.EqualTo(150));
        }

        [Test]
        public void Should_refuse_a_second_update_while_one_is_pending()
        {
            var order = CreateOrder(State.Draft, 100);
            Update(order, 200);

            var result = Update(order, 300);

            Assert.That(result.IsFailure(), Is.True);
            Assert.That(order.AcceptPending(new TriggerContext()).IsSuccess(), Is.True);
            Assert.That(order.Quantity, Is.EqualTo(200));
        }

        [Test]
        public void Should_refuse_an_update_with_invalid_data()
        {
            var order = CreateOrder(State.Draft, 100);
            var eventCount = order.EventLogs.Count;

            var result = Update(order, 0);

            Assert.That(result.IsFailure(), Is.True);
            Assert.That(order.PendingTrigger, Is.Null);
            Assert.That(order.EventLogs.Count, Is.EqualTo(eventCount));
        }

        [Test]
        public void Should_rebuild_a_pending_and_an_accepted_update_from_the_event_log()
        {
            var order = CreateOrder(State.Draft, 100);
            Update(order, 200, DateTime.Now.AddMinutes(10));

            var pending = new Order { EventLogs = order.EventLogs.ToList() };
            Assert.That(pending.Quantity, Is.EqualTo(100));
            Assert.That(pending.PendingTrigger, Is.EqualTo(OrderStateMachine.Trigger.Update));

            order.AcceptPending(new TriggerContext(new User(), DateTime.Now.AddMinutes(20)));
            var accepted = new Order { EventLogs = order.EventLogs.ToList() };

            Assert.That(accepted.Quantity, Is.EqualTo(200));
            Assert.That(accepted.PendingTrigger, Is.Null);
        }
    }
}
