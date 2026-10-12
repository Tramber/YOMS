using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Oms.Client.Modules.OrderEditor;
using Oms.Client.Services;
using Oms.Transport.Contracts.Dto;
using Oms.Transport.Contracts.Messages;
using Oms.Transport.Contracts.Messages.Orders;

namespace Oms.Client.Tests.Modules
{
    [TestFixture]
    public class OrderEditorViewModelTests
    {
        private IOrderServiceProxy _proxy;
        private Session _session;
        private OrderEditorViewModel _editor;
        private OrderEditionRequest _sent;
        private int _closed;

        [SetUp]
        public void SetUp()
        {
            _proxy = Substitute.For<IOrderServiceProxy>();
            _proxy.GetSecuritiesAsync().Returns(Task.FromResult(new List<SecurityDto> { new SecurityDto { Id = 1, Name = "ACME" }, new SecurityDto { Id = 2, Name = "Globex" } }));
            _proxy.GetFundsAsync().Returns(Task.FromResult(new List<FundDto> { new FundDto { Id = 5, Name = "Alpha" } }));
            _session = new Session { Login = "alice" };
            _editor = new OrderEditorViewModel(_proxy, _session);
            _closed = 0;
            _editor.CloseRequested += (_, __) => _closed++;
            _sent = null;
        }

        private void ServerAnswers(GenericResultDto result)
        {
            _proxy.SendAsync(Arg.Do<OrderEditionRequest>(r => _sent = r)).Returns(call =>
                Task.FromResult(new Response<Tuple<OrderDto, GenericResultDto>, EditionCommand>(
                    call.Arg<OrderEditionRequest>().Command,
                    new List<Tuple<OrderDto, GenericResultDto>> { Tuple.Create(new OrderDto(), result) })));
        }

        private async Task FillAsync(decimal quantity = 100, decimal? price = 12.5m)
        {
            await _editor.LoadAsync();
            _editor.Quantity = quantity;
            _editor.PriceLimit = price;
        }

        [Test]
        public async Task Should_load_the_securities_and_funds_and_select_the_first_of_each()
        {
            await _editor.LoadAsync();

            Assert.That(_editor.Securities.Select(s => s.Name), Is.EqualTo(new[] { "ACME", "Globex" }));
            Assert.That(_editor.SelectedSecurity.Name, Is.EqualTo("ACME"));
            Assert.That(_editor.SelectedFund.Name, Is.EqualTo("Alpha"));
        }

        [Test]
        public async Task Should_select_nothing_when_the_server_has_no_securities_or_funds()
        {
            _proxy.GetSecuritiesAsync().Returns(Task.FromResult(new List<SecurityDto>()));
            _proxy.GetFundsAsync().Returns(Task.FromResult(new List<FundDto>()));

            await _editor.LoadAsync();

            Assert.That(_editor.SelectedSecurity, Is.Null);
            Assert.That(_editor.SelectedFund, Is.Null);
        }

        [Test]
        public void Should_default_to_a_day_limit_buy_order()
        {
            Assert.That(_editor.SelectedWay, Is.EqualTo(OrderDtoWay.Buy));
            Assert.That(_editor.SelectedOrderType, Is.EqualTo(OrderDtoType.Limit));
            Assert.That(_editor.SelectedValidity, Is.EqualTo(OrderDtoValidityType.Day));
            Assert.That(_editor.IsLimit, Is.True);
        }

        [Test]
        public async Task Should_need_a_security_a_fund_and_a_quantity_before_it_can_be_sent()
        {
            Assert.That(_editor.SendCommand.CanExecute(null), Is.False);

            await _editor.LoadAsync();
            Assert.That(_editor.SendCommand.CanExecute(null), Is.False);

            _editor.Quantity = 100;
            _editor.PriceLimit = 10;
            Assert.That(_editor.SendCommand.CanExecute(null), Is.True);
            Assert.That(_editor.StashCommand.CanExecute(null), Is.True);
        }

        [TestCase(0)]
        [TestCase(-5)]
        public async Task Should_refuse_a_quantity_that_is_not_positive(int quantity)
        {
            await FillAsync(quantity);

            Assert.That(_editor.SendCommand.CanExecute(null), Is.False);
        }

        [Test]
        public async Task Should_need_a_price_for_a_limit_order_only()
        {
            await FillAsync(100, null);
            Assert.That(_editor.SendCommand.CanExecute(null), Is.False);

            _editor.SelectedOrderType = OrderDtoType.Market;

            Assert.That(_editor.IsLimit, Is.False);
            Assert.That(_editor.SendCommand.CanExecute(null), Is.True);
        }

        [Test]
        public async Task Should_send_the_order_for_acceptance_and_close_when_the_server_accepts_it()
        {
            await FillAsync(250, 12.5m);
            _editor.SelectedSecurity = _editor.Securities[1];
            _editor.SelectedWay = OrderDtoWay.Sell;
            _editor.SelectedValidity = OrderDtoValidityType.Gtc;
            _editor.ClientOrderRef = "my-ref";
            ServerAnswers(new GenericResultDto(null, 0));

            await _editor.SendCommand.ExecuteAsync(null);

            Assert.That(_sent.Command, Is.EqualTo(EditionCommand.Create));
            Assert.That(_sent.Token, Is.EqualTo("alice"));
            var dto = _sent.ItemList.Single();
            Assert.That(dto.Security.Id, Is.EqualTo(2));
            Assert.That(dto.Fund.Id, Is.EqualTo(5));
            Assert.That(dto.Way, Is.EqualTo(OrderDtoWay.Sell));
            Assert.That(dto.OrderType, Is.EqualTo(OrderDtoType.Limit));
            Assert.That(dto.Validity, Is.EqualTo(OrderDtoValidityType.Gtc));
            Assert.That(dto.Quantity, Is.EqualTo(250));
            Assert.That(dto.PriceLimit, Is.EqualTo(12.5));
            Assert.That(dto.ClientOrderRef, Is.EqualTo("my-ref"));
            Assert.That(_closed, Is.EqualTo(1));
            Assert.That(_editor.ErrorMessage, Is.Null);
        }

        [Test]
        public async Task Should_stash_the_order_as_a_draft()
        {
            await FillAsync();
            ServerAnswers(new GenericResultDto(null, 0));

            await _editor.StashCommand.ExecuteAsync(null);

            Assert.That(_sent.Command, Is.EqualTo(EditionCommand.Stash));
            Assert.That(_closed, Is.EqualTo(1));
        }

        [Test]
        public async Task Should_not_send_a_price_for_a_market_order()
        {
            await FillAsync(100, 99);
            _editor.SelectedOrderType = OrderDtoType.Market;
            ServerAnswers(new GenericResultDto(null, 0));

            await _editor.SendCommand.ExecuteAsync(null);

            Assert.That(_sent.ItemList.Single().OrderType, Is.EqualTo(OrderDtoType.Market));
            Assert.That(_sent.ItemList.Single().PriceLimit, Is.Null);
        }

        [Test]
        public async Task Should_show_the_message_of_the_server_and_stay_open_when_it_refuses_the_order()
        {
            await FillAsync();
            ServerAnswers(new GenericResultDto("Quantity : must be greater than 0", 1));

            await _editor.SendCommand.ExecuteAsync(null);

            Assert.That(_editor.ErrorMessage, Is.EqualTo("Quantity : must be greater than 0"));
            Assert.That(_closed, Is.EqualTo(0));
            Assert.That(_editor.SendCommand.CanExecute(null), Is.True);
        }

        [Test]
        public async Task Should_show_the_problem_and_stay_open_when_the_server_cannot_be_reached()
        {
            await FillAsync();
            _proxy.SendAsync(Arg.Any<OrderEditionRequest>()).Throws(new InvalidOperationException("connection lost"));

            await _editor.SendCommand.ExecuteAsync(null);

            Assert.That(_editor.ErrorMessage, Is.EqualTo("connection lost"));
            Assert.That(_closed, Is.EqualTo(0));
            Assert.That(_editor.IsBusy, Is.False);
        }

        [Test]
        public async Task Should_clear_the_previous_error_when_sending_again()
        {
            await FillAsync();
            ServerAnswers(new GenericResultDto("refused", 1));
            await _editor.SendCommand.ExecuteAsync(null);
            Assert.That(_editor.ErrorMessage, Is.EqualTo("refused"));

            ServerAnswers(new GenericResultDto(null, 0));
            await _editor.SendCommand.ExecuteAsync(null);

            Assert.That(_editor.ErrorMessage, Is.Null);
            Assert.That(_closed, Is.EqualTo(1));
        }

        [Test]
        public void Should_close_without_sending_anything()
        {
            _editor.CloseCommand.Execute(null);

            Assert.That(_closed, Is.EqualTo(1));
            _proxy.DidNotReceive().SendAsync(Arg.Any<OrderEditionRequest>());
        }
    }
}
