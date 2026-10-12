using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Oms.Server.Core.Contracts;
using Oms.Server.Core.Services;
using Oms.Server.Domain.Framework;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.Funds;
using Oms.Server.Domain.Models.Orders;
using Oms.Server.Domain.Models.Securities;
using Oms.Transport.Contracts;
using Oms.Transport.Contracts.Dto;
using Oms.Transport.Contracts.Messages;
using Oms.Transport.Contracts.Messages.Orders;

namespace Oms.Server.Core.Tests.Contracts
{
    [TestFixture]
    public class OrderServiceContractTests
    {
        private IOrderManagementService _service;
        private OrderServiceContract _contract;
        private List<Tuple<ItemOperationType, OrderDto[]>> _published;

        [SetUp]
        public void SetUp()
        {
            _service = Substitute.For<IOrderManagementService>();
            _contract = new OrderServiceContract(_service);
            _published = new List<Tuple<ItemOperationType, OrderDto[]>>();
            _contract.OrderListUpdated += (type, orders) => _published.Add(Tuple.Create(type, orders));
        }

        private static OrderCommandResult Success(int id)
        {
            return new OrderCommandResult(id, new Order { Id = id }, GenericResult.Success());
        }

        private static OrderCommandResult Failure(int id, string message = "refused")
        {
            return new OrderCommandResult(id, null, GenericResult.Failure(message));
        }

        private static OrderRequest StateRequest(OrderStateCommand command, params int[] ids)
        {
            return new OrderRequest("token", command, new List<int>(ids), default(TransactionMode));
        }

        [Test]
        public void Should_answer_an_edition_with_the_saved_orders_and_publish_them_as_added()
        {
            var request = new OrderEditionRequest("token", EditionCommand.Create, new List<OrderDto> { new OrderDto { Quantity = 5 } }, default(TransactionMode));
            _service.HandleRequest(request).Returns(new List<OrderCommandResult> { Success(11) });

            var response = _contract.Handle(request);

            Assert.That(response.Command, Is.EqualTo(EditionCommand.Create));
            Assert.That(response.ResultList.Single().Item1.Id, Is.EqualTo(11));
            Assert.That(response.ResultList.Single().Item2.ErrorCode, Is.EqualTo(GenericResult.NoErrorCode));
            Assert.That(_published.Single().Item1, Is.EqualTo(ItemOperationType.Add));
            Assert.That(_published.Single().Item2.Single().Id, Is.EqualTo(11));
        }

        [Test]
        public void Should_publish_an_edited_order_as_updated()
        {
            var request = new OrderEditionRequest("token", EditionCommand.Update, new List<OrderDto> { new OrderDto { Id = 4 } }, default(TransactionMode));
            _service.HandleRequest(request).Returns(new List<OrderCommandResult> { Success(4) });

            _contract.Handle(request);

            Assert.That(_published.Single().Item1, Is.EqualTo(ItemOperationType.Update));
        }

        [Test]
        public void Should_answer_a_failed_edition_with_the_submitted_order_and_the_error_and_publish_nothing()
        {
            var submitted = new OrderDto { Quantity = 0 };
            var request = new OrderEditionRequest("token", EditionCommand.Create, new List<OrderDto> { submitted }, default(TransactionMode));
            _service.HandleRequest(request).Returns(new List<OrderCommandResult> { Failure(0, "Quantity must be greater than 0") });

            var response = _contract.Handle(request);

            Assert.That(response.ResultList.Single().Item1, Is.SameAs(submitted));
            Assert.That(response.ResultList.Single().Item2.ErrorMessage, Is.EqualTo("Quantity must be greater than 0"));
            Assert.That(response.ResultList.Single().Item2.ErrorCode, Is.Not.EqualTo(GenericResult.NoErrorCode));
            Assert.That(_published, Is.Empty);
        }

        [Test]
        public void Should_answer_a_state_request_with_one_outcome_per_order_and_publish_only_the_changes()
        {
            var request = StateRequest(OrderStateCommand.Cancel, 1, 2);
            _service.HandleRequest(request).Returns(new List<OrderCommandResult> { Success(1), Failure(2) });

            var response = _contract.Handle(request);

            Assert.That(response.ResultList.Select(r => r.Item1), Is.EqualTo(new[] { 1, 2 }));
            Assert.That(response.ResultList[0].Item2.ErrorCode, Is.EqualTo(GenericResult.NoErrorCode));
            Assert.That(response.ResultList[1].Item2.ErrorMessage, Is.EqualTo("refused"));
            Assert.That(_published.Single().Item1, Is.EqualTo(ItemOperationType.Update));
            Assert.That(_published.Single().Item2.Select(o => o.Id), Is.EqualTo(new[] { 1 }));
        }

        [Test]
        public void Should_publish_a_deleted_order_as_deleted()
        {
            var request = StateRequest(OrderStateCommand.Delete, 1);
            _service.HandleRequest(request).Returns(new List<OrderCommandResult> { Success(1) });

            _contract.Handle(request);

            Assert.That(_published.Single().Item1, Is.EqualTo(ItemOperationType.Delete));
        }

        [Test]
        public void Should_answer_a_market_request_and_publish_the_orders_as_updated()
        {
            var request = new OrderMarketRequest("token", OrderMarketCommand.MarketSend, new List<int> { 3 }, default(TransactionMode));
            _service.HandleRequest(request).Returns(new List<OrderCommandResult> { Success(3) });

            var response = _contract.Handle(request);

            Assert.That(response.Command, Is.EqualTo(OrderMarketCommand.MarketSend));
            Assert.That(response.ResultList.Single().Item1, Is.EqualTo(3));
            Assert.That(_published.Single().Item1, Is.EqualTo(ItemOperationType.Update));
        }

        [Test]
        public void Should_publish_nothing_when_every_order_failed()
        {
            var request = StateRequest(OrderStateCommand.Cancel, 1);
            _service.HandleRequest(request).Returns(new List<OrderCommandResult> { Failure(1) });

            _contract.Handle(request);

            Assert.That(_published, Is.Empty);
        }

        [Test]
        public void Should_list_the_orders_securities_and_funds_as_dtos()
        {
            _service.GetOrders().Returns(new List<Order> { new Order { Id = 8 } });
            _service.GetSecurities().Returns(new List<Security> { new Security { Id = 9, Name = "ACME", Currency = Currency.Eur, Type = SecurityType.Equity } });
            _service.GetFunds().Returns(new List<IFund> { new Fund { Id = 10, Name = "Alpha" } });

            Assert.That(_contract.GetOrders().Single().Id, Is.EqualTo(8));
            Assert.That(_contract.GetSecurities().Single().Name, Is.EqualTo("ACME"));
            Assert.That(_contract.GetFunds().Single().Name, Is.EqualTo("Alpha"));
        }

        [Test]
        public void Should_work_without_subscribers()
        {
            var contract = new OrderServiceContract(_service);
            var request = StateRequest(OrderStateCommand.Cancel, 1);
            _service.HandleRequest(request).Returns(new List<OrderCommandResult> { Success(1) });

            Assert.DoesNotThrow(() => contract.Handle(request));
        }
    }
}
