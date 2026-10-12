using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Oms.Client.Model;
using Oms.Client.Modules.OrderBlotter;
using Oms.Client.Services;
using Oms.Client.Services.Notifications;
using Oms.Transport.Contracts;
using Oms.Transport.Contracts.Dto;
using Oms.Transport.Contracts.Messages;
using Oms.Transport.Contracts.Messages.Orders;

namespace Oms.Client.Tests.Modules
{
    [TestFixture]
    public class OrderBlotterViewModelTests
    {
        private IOrderServiceProxy _proxy;
        private INotificationService _notifications;
        private Session _session;
        private OrderBlotterViewModel _blotter;
        private List<OrderAdapter> _edited;

        [SetUp]
        public void SetUp()
        {
            _proxy = Substitute.For<IOrderServiceProxy>();
            _notifications = Substitute.For<INotificationService>();
            _session = new Session { Login = "trader" };
            _proxy.GetOrdersAsync().Returns(Task.FromResult(new List<OrderDto>()));
            _edited = [];
            _blotter = new OrderBlotterViewModel(_proxy, _notifications, _session, order => { _edited.Add(order); return Task.CompletedTask; });
        }

        private static OrderDto Dto(int id, double quantity = 100, string state = "Working", string status = "New")
        {
            return new OrderDto
            {
                Id = id,
                ClientOrderRef = "ref-" + id,
                Quantity = quantity,
                RemainingQuantity = quantity,
                PriceLimit = 12.5,
                Way = OrderDtoWay.Sell,
                OrderType = OrderDtoType.Limit,
                State = state,
                Status = status,
                Security = new SecurityDto { Id = 1, Name = "ACME", IsinCode = "FR0000000001" },
            };
        }

        private OrderAdapter[] Rows()
        {
            return [.. _blotter.OrdersView.Cast<object>().OfType<OrderAdapter>()];
        }

        private async Task LoadAsync(params OrderDto[] orders)
        {
            _proxy.GetOrdersAsync().Returns(Task.FromResult(orders.ToList()));
            await _blotter.LoadAsync();
        }

        private static GenericResultDto Ok { get { return new GenericResultDto(null, 0); } }

        private static GenericResultDto Refused(string message) { return new GenericResultDto(message, 1); }

        private void ServerAnswersState(OrderStateCommand command, params Tuple<int, GenericResultDto>[] results)
        {
            _proxy.SendAsync(Arg.Any<OrderRequest>())
                .Returns(Task.FromResult(new Response<Tuple<int, GenericResultDto>, OrderStateCommand>(command,
                    [.. results])));
        }

        private void ServerAnswersMarket(OrderMarketCommand command, params Tuple<int, GenericResultDto>[] results)
        {
            _proxy.SendAsync(Arg.Any<OrderMarketRequest>())
                .Returns(Task.FromResult(new Response<Tuple<int, GenericResultDto>, OrderMarketCommand>(command,
                    [.. results])));
        }

        private void ServerAnswersTrade(OrderTradeCommand command, params Tuple<int, GenericResultDto>[] results)
        {
            _proxy.SendAsync(Arg.Any<OrderTradeRequest>())
                .Returns(Task.FromResult(new Response<Tuple<int, GenericResultDto>, OrderTradeCommand>(command,
                    [.. results])));
        }

        [Test]
        public void Should_follow_the_changes_pushed_by_the_server()
        {
            _proxy.Received(1).Subscribe(Arg.Any<IObserver<Tuple<ItemOperationType, OrderDto[]>>>());
        }

        [Test]
        public async Task Should_show_the_orders_of_the_server()
        {
            await LoadAsync(Dto(1, 100), Dto(2, 50, "Draft", "Pending"));

            var rows = Rows();
            Assert.That(rows.Select(r => r.Id), Is.EqualTo([1, 2]));
            Assert.That(rows[0].ClientOrderRef, Is.EqualTo("ref-1"));
            Assert.That(rows[0].SecurityName, Is.EqualTo("ACME"));
            Assert.That(rows[0].Way, Is.EqualTo(OrderDtoWay.Sell));
            Assert.That(rows[0].Quantity, Is.EqualTo(100));
            Assert.That(rows[0].RemainingQuantity, Is.EqualTo(100));
            Assert.That(rows[0].Price, Is.EqualTo(12.5));
            Assert.That(rows[1].State, Is.EqualTo("Draft"));
            Assert.That(rows[1].Status, Is.EqualTo("Pending"));
        }

        [Test]
        public async Task Should_start_without_orders_and_show_nothing_when_the_server_has_none()
        {
            Assert.That(Rows(), Is.Empty);

            await LoadAsync();

            Assert.That(Rows(), Is.Empty);
        }

        [Test]
        public async Task Should_replace_the_rows_when_loading_again_and_keep_their_local_group()
        {
            await LoadAsync(Dto(1), Dto(2));
            _blotter.GroupOrdersCommand.Execute(Rows().Where(r => r.Id == 1).ToList());
            var group = Rows().Single(r => r.Id == 1).GroupId;
            Assert.That(group, Is.GreaterThanOrEqualTo(4000));

            await LoadAsync(Dto(1), Dto(3));

            Assert.That(Rows().Select(r => r.Id), Is.EqualTo([1, 3]));
            Assert.That(Rows().Single(r => r.Id == 1).GroupId, Is.EqualTo(group));
            Assert.That(Rows().Single(r => r.Id == 3).GroupId, Is.EqualTo(0));
        }

        [Test]
        public void Should_add_an_order_pushed_by_the_server()
        {
            _blotter.Apply(ItemOperationType.Add, [Dto(7)]);

            Assert.That(Rows().Single().Id, Is.EqualTo(7));
        }

        [Test]
        public async Task Should_update_an_existing_row_in_place()
        {
            await LoadAsync(Dto(1, 100, "Accepting", "New"));
            var row = Rows().Single();
            var changed = new List<string>();
            row.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            _blotter.Apply(ItemOperationType.Update, [Dto(1, 150, "Working", "CancelPending")]);

            Assert.That(Rows().Single(), Is.SameAs(row));
            Assert.That(row.State, Is.EqualTo("Working"));
            Assert.That(row.Status, Is.EqualTo("CancelPending"));
            Assert.That(row.Quantity, Is.EqualTo(150));
            Assert.That(changed, Does.Contain("Status").And.Contain("State").And.Contain("Quantity"));
        }

        [Test]
        public void Should_show_an_updated_order_it_did_not_know()
        {
            _blotter.Apply(ItemOperationType.Update, [Dto(9)]);

            Assert.That(Rows().Single().Id, Is.EqualTo(9));
        }

        [TestCase(ItemOperationType.Delete)]
        [TestCase(ItemOperationType.Remove)]
        public async Task Should_remove_the_orders_that_the_server_deleted(ItemOperationType type)
        {
            await LoadAsync(Dto(1), Dto(2));

            _blotter.Apply(type, [Dto(1), Dto(5)]);

            Assert.That(Rows().Select(r => r.Id), Is.EqualTo([2]));
        }

        [Test]
        public async Task Should_group_ungrouped_orders_under_a_new_group_id()
        {
            await LoadAsync(Dto(1), Dto(2), Dto(3));
            var selected = Rows().Where(r => r.Id <= 2).ToList();

            _blotter.GroupOrdersCommand.Execute(selected);

            Assert.That(selected.Select(o => o.GroupId).Distinct().Count(), Is.EqualTo(1));
            Assert.That(selected[0].GroupId, Is.GreaterThanOrEqualTo(4000));
            Assert.That(Rows().Single(r => r.Id == 3).GroupId, Is.EqualTo(0));
        }

        [Test]
        public async Task Should_not_group_orders_when_one_is_already_grouped()
        {
            await LoadAsync(Dto(1), Dto(2));
            _blotter.GroupOrdersCommand.Execute(Rows().Where(r => r.Id == 1).ToList());
            var grouped = Rows().Single(r => r.Id == 1).GroupId;

            _blotter.GroupOrdersCommand.Execute(Rows().ToList());

            Assert.That(Rows().Single(r => r.Id == 1).GroupId, Is.EqualTo(grouped));
            Assert.That(Rows().Single(r => r.Id == 2).GroupId, Is.EqualTo(0));
        }

        [Test]
        public async Task Should_ungroup_grouped_orders()
        {
            await LoadAsync(Dto(1), Dto(2));
            _blotter.GroupOrdersCommand.Execute(Rows().ToList());

            _blotter.UnGroupOrdersCommand.Execute(Rows().ToList());

            Assert.That(Rows().All(o => o.GroupId == 0), Is.True);
        }

        [Test]
        public async Task Should_filter_the_rows_with_a_property_expression()
        {
            await LoadAsync(Dto(1), Dto(2));

            _blotter.FilterText = "ClientOrderRef = \"ref-2\"";
            Assert.That(Rows().Select(r => r.Id), Is.EqualTo([2]));

            _blotter.FilterText = "Security.IsinCode = \"FR0000000001\"";
            Assert.That(Rows(), Has.Length.EqualTo(2));

            _blotter.FilterText = "Security.IsinCode = \"XX\"";
            Assert.That(Rows(), Is.Empty);
        }

        [TestCase("not an expression")]
        [TestCase("Id = \"5\"")]
        [TestCase("")]
        public async Task Should_show_every_row_when_the_filter_is_empty_or_invalid(string text)
        {
            await LoadAsync(Dto(1), Dto(2));
            _blotter.FilterText = "ClientOrderRef = \"none\"";

            _blotter.FilterText = text;

            Assert.That(Rows(), Has.Length.EqualTo(2));
        }

        [Test]
        public async Task Should_treat_a_missing_nested_value_as_a_non_match()
        {
            var noSecurity = Dto(1);
            noSecurity.Security = null;
            await LoadAsync(noSecurity);

            _blotter.FilterText = "Security.IsinCode = \"FR0000000001\"";

            Assert.That(Rows(), Is.Empty);
        }

        [Test]
        public async Task Should_send_the_selected_order_ids_with_the_login_for_a_state_command()
        {
            await LoadAsync(Dto(1), Dto(2), Dto(3));
            ServerAnswersState(OrderStateCommand.Cancel, Tuple.Create(1, Ok), Tuple.Create(3, Ok));
            _session.Login = "alice";

            await _blotter.CancelOrdersCommand.ExecuteAsync(Rows().Where(r => r.Id != 2).ToList());

            await _proxy.Received(1).SendAsync(Arg.Is<OrderRequest>(r =>
                r.Command == OrderStateCommand.Cancel && r.Token == "alice" && r.ItemList.SequenceEqual(new[] { 1, 3 })));
            _notifications.DidNotReceive().Process(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<NotificationType>());
        }

        [Test]
        public async Task Should_map_each_toolbar_button_to_its_state_command()
        {
            await LoadAsync(Dto(1));
            ServerAnswersState(OrderStateCommand.Recall, Tuple.Create(1, Ok));
            var selected = Rows().ToList();

            await _blotter.RecallOrdersCommand.ExecuteAsync(selected);
            await _blotter.DeleteOrdersCommand.ExecuteAsync(selected);
            await _blotter.AcceptPendingCommand.ExecuteAsync(selected);
            await _blotter.RejectPendingCommand.ExecuteAsync(selected);

            foreach (var command in new[] { OrderStateCommand.Recall, OrderStateCommand.Delete, OrderStateCommand.AcceptPending, OrderStateCommand.RejectPending })
            {
                var expected = command;
                await _proxy.Received(1).SendAsync(Arg.Is<OrderRequest>(r => r.Command == expected));
            }
        }

        [Test]
        public async Task Should_send_market_commands_through_the_market_request()
        {
            await LoadAsync(Dto(4));
            ServerAnswersMarket(OrderMarketCommand.MarketSend, Tuple.Create(4, Ok));
            var selected = Rows().ToList();

            await _blotter.SendToMarketCommand.ExecuteAsync(selected);
            await _blotter.CancelAtMarketCommand.ExecuteAsync(selected);

            await _proxy.Received(1).SendAsync(Arg.Is<OrderMarketRequest>(r =>
                r.Command == OrderMarketCommand.MarketSend && r.Token == "trader" && r.ItemList.SequenceEqual(new[] { 4 })));
            await _proxy.Received(1).SendAsync(Arg.Is<OrderMarketRequest>(r => r.Command == OrderMarketCommand.MarketCancel));
        }

        [Test]
        public async Task Should_report_only_the_orders_the_server_refused()
        {
            await LoadAsync(Dto(1), Dto(2));
            ServerAnswersState(OrderStateCommand.Cancel, Tuple.Create(1, Ok), Tuple.Create(2, Refused("The command Cancel is not allowed")));

            await _blotter.CancelOrdersCommand.ExecuteAsync(Rows().ToList());

            _notifications.Received(1).Process("Cancel: order 2", "The command Cancel is not allowed", NotificationType.Error);
            _notifications.Received(1).Process(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<NotificationType>());
        }

        [Test]
        public async Task Should_report_a_market_refusal()
        {
            await LoadAsync(Dto(1));
            ServerAnswersMarket(OrderMarketCommand.MarketSend, Tuple.Create(1, Refused("not working")));

            await _blotter.SendToMarketCommand.ExecuteAsync(Rows().ToList());

            _notifications.Received(1).Process("Send to market: order 1", "not working", NotificationType.Error);
        }

        [Test]
        public async Task Should_report_a_server_that_cannot_be_reached()
        {
            await LoadAsync(Dto(1));
            _proxy.SendAsync(Arg.Any<OrderRequest>()).Throws(new InvalidOperationException("connection lost"));

            await _blotter.DeleteOrdersCommand.ExecuteAsync(Rows().ToList());

            _notifications.Received(1).Process("Delete", "connection lost", NotificationType.Error);
        }

        [Test]
        public async Task Should_send_nothing_when_no_order_is_selected()
        {
            await _blotter.CancelOrdersCommand.ExecuteAsync(new List<OrderAdapter>());
            await _blotter.SendToMarketCommand.ExecuteAsync(new List<OrderAdapter>());

            await _proxy.DidNotReceive().SendAsync(Arg.Any<OrderRequest>());
            await _proxy.DidNotReceive().SendAsync(Arg.Any<OrderMarketRequest>());
        }

        [Test]
        public async Task Should_execute_what_remains_of_the_selected_orders_at_the_limit_price()
        {
            var partial = Dto(1, 100);
            partial.RemainingQuantity = 40;
            await LoadAsync(partial, Dto(2, 50));
            ServerAnswersTrade(OrderTradeCommand.AddTrade, Tuple.Create(1, Ok), Tuple.Create(2, Ok));

            await _blotter.ExecuteOrdersCommand.ExecuteAsync(Rows().ToList());

            await _proxy.Received(1).SendAsync(Arg.Is<OrderTradeRequest>(r =>
                r.Command == OrderTradeCommand.AddTrade && r.Token == "trader" && r.ItemList.Count == 2
                && r.ItemList[0].OrderId == 1 && r.ItemList[0].ExecutionQuantity == 40 && r.ItemList[0].ExecutionAmountNet == 500
                && r.ItemList[0].Way == OrderDtoWay.Sell
                && r.ItemList[1].OrderId == 2 && r.ItemList[1].ExecutionQuantity == 50));
        }

        [Test]
        public async Task Should_book_the_trades_that_are_not_booked_yet()
        {
            var order = Dto(1);
            order.Trades =
            [
                new TradeDto { Id = 10, OrderId = 1, State = "New" },
                new TradeDto { Id = 11, OrderId = 1, State = "Booked" },
                new TradeDto { Id = 12, OrderId = 1, State = "Cancelled" },
                new TradeDto { Id = 13, OrderId = 1, State = "BookingError" }
            ];
            await LoadAsync(order);
            ServerAnswersTrade(OrderTradeCommand.BookTrade, Tuple.Create(1, Ok), Tuple.Create(1, Ok));

            await _blotter.BookTradesCommand.ExecuteAsync(Rows().ToList());

            await _proxy.Received(1).SendAsync(Arg.Is<OrderTradeRequest>(r =>
                r.Command == OrderTradeCommand.BookTrade && r.ItemList.Select(t => t.Id).SequenceEqual(new[] { 10, 13 })));
        }

        [Test]
        public async Task Should_start_the_booking_through_the_state_request()
        {
            await LoadAsync(Dto(3));
            ServerAnswersState(OrderStateCommand.StartBooking, Tuple.Create(3, Ok));

            await _blotter.StartBookingCommand.ExecuteAsync(Rows().ToList());

            await _proxy.Received(1).SendAsync(Arg.Is<OrderRequest>(r => r.Command == OrderStateCommand.StartBooking && r.ItemList.SequenceEqual(new[] { 3 })));
        }

        [Test]
        public async Task Should_report_a_refused_trade_and_send_nothing_when_there_is_nothing_to_execute_or_book()
        {
            var filled = Dto(1);
            filled.RemainingQuantity = 0;
            await LoadAsync(filled, Dto(2));
            ServerAnswersTrade(OrderTradeCommand.AddTrade, Tuple.Create(2, Refused("not allowed")));

            await _blotter.BookTradesCommand.ExecuteAsync(Rows().ToList());
            await _proxy.DidNotReceive().SendAsync(Arg.Any<OrderTradeRequest>());

            await _blotter.ExecuteOrdersCommand.ExecuteAsync(Rows().ToList());

            _notifications.Received(1).Process("Execute: order 2", "not allowed", NotificationType.Error);
        }

        [Test]
        public async Task Should_open_the_editor_on_the_single_selected_order()
        {
            await LoadAsync(Dto(1), Dto(2));

            await _blotter.EditOrderCommand.ExecuteAsync(Rows().Where(o => o.Id == 2).ToList());

            Assert.That(_edited.Single().Id, Is.EqualTo(2));
        }

        [Test]
        public async Task Should_ask_for_a_single_order_when_several_or_none_are_selected()
        {
            await LoadAsync(Dto(1), Dto(2));

            await _blotter.EditOrderCommand.ExecuteAsync(new List<OrderAdapter>());
            _notifications.DidNotReceive().Process(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<NotificationType>());

            await _blotter.EditOrderCommand.ExecuteAsync(Rows().ToList());

            Assert.That(_edited, Is.Empty);
            _notifications.Received(1).Process("Edit", "Select a single order to edit", NotificationType.Error);
        }
    }
}
