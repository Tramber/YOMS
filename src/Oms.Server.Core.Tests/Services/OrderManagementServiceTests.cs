using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using Oms.Server.Core.Services;
using Oms.Server.Domain.Framework;
using Oms.Server.Domain.Interfaces.Repository;
using Oms.Server.Domain.Models.EventLogs;
using Oms.Server.Domain.Models.Orders;
using Oms.Server.Domain.Workflow;
using Oms.Transport.Contracts.Messages;
using Oms.Transport.Contracts.Messages.Orders;

namespace Oms.Server.Core.Tests.Services
{
    [TestFixture]
    public class OrderManagementServiceTests
    {
        private IRepositoryFacade _repositories;
        private IOrderRepository _orders;
        private OrderManagementService _service;

        [SetUp]
        public void SetUp()
        {
            _orders = Substitute.For<IOrderRepository>();
            _repositories = Substitute.For<IRepositoryFacade>();
            _repositories.Orders.Returns(_orders);
            _service = new OrderManagementService(_repositories);
        }

        private static Order AcceptingOrder(int id)
        {
            var order = new Order { Id = id };
            Assert.That(order.SendRequest(new TriggerContext()).IsSuccess(), Is.True);
            return order;
        }

        private static OrderRequest Request(OrderStateCommand command, params int[] ids)
        {
            return new OrderRequest("token", command, new List<int>(ids), default(TransactionMode));
        }

        [Test]
        public void Should_apply_the_command_to_the_loaded_order_and_save_it()
        {
            var order = AcceptingOrder(1);
            _orders.GetById(1).Returns(order);

            var result = _service.HandleRequest(Request(OrderStateCommand.Recall, 1));

            Assert.That(result.IsSuccess(), Is.True);
            Assert.That(order.OrderState, Is.EqualTo(OrderStateMachine.State.Draft));
            _orders.Received(1).Update(order);
        }

        [Test]
        public void Should_handle_every_order_of_the_request()
        {
            var first = AcceptingOrder(1);
            var second = AcceptingOrder(2);
            _orders.GetById(1).Returns(first);
            _orders.GetById(2).Returns(second);

            _service.HandleRequest(Request(OrderStateCommand.Recall, 1, 2));

            Assert.That(first.OrderState, Is.EqualTo(OrderStateMachine.State.Draft));
            Assert.That(second.OrderState, Is.EqualTo(OrderStateMachine.State.Draft));
            _orders.Received(1).Update(first);
            _orders.Received(1).Update(second);
        }

        [Test]
        public void Should_not_save_an_order_when_the_command_is_refused()
        {
            var order = AcceptingOrder(1);
            _orders.GetById(1).Returns(order);

            _service.HandleRequest(Request(OrderStateCommand.Delete, 1));

            Assert.That(order.OrderState, Is.EqualTo(OrderStateMachine.State.Accepting));
            _orders.DidNotReceive().Update(Arg.Any<Order>());
        }

        [Test]
        public void Should_refuse_a_null_request()
        {
            Assert.Throws<System.ArgumentNullException>(() => _service.HandleRequest((OrderRequest)null));
        }

        [Test]
        public void Should_do_nothing_for_an_empty_request()
        {
            var result = _service.HandleRequest(Request(OrderStateCommand.Recall));

            Assert.That(result.IsSuccess(), Is.True);
            _orders.DidNotReceive().Update(Arg.Any<Order>());
        }
    }
}
