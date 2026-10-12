using System.Linq;
using NUnit.Framework;
using Oms.Server.Domain.Framework;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.EventLogs;
using Oms.Server.Domain.Models.Trades;

namespace Oms.Server.Domain.Tests.Models.Trades
{
    [TestFixture]
    public class TradeTests
    {
        private static TradeEditableEventParameter Data(double quantity = 100, double amount = 1500, Side side = Side.Sell)
        {
            return new TradeEditableEventParameter { ExecutionQuantity = quantity, ExecutionAmountNet = amount, Side = side };
        }

        [Test]
        public void Should_create_a_new_trade_from_the_editable_data()
        {
            var trade = new Trade();

            var result = trade.Create(new TriggerContext(), Data());

            Assert.That(result.IsSuccess(), Is.True, result.ErrorMessage);
            Assert.That(trade.TradeState, Is.EqualTo(TradeState.New));
            Assert.That(trade.ExecutionQuantity, Is.EqualTo(100));
            Assert.That(trade.ExecutionAmountNet, Is.EqualTo(1500));
            Assert.That(trade.Side, Is.EqualTo(Side.Sell));
            var log = trade.EventLogs.Single();
            Assert.That(log.Trigger, Is.EqualTo(TradeTrigger.Create));
            Assert.That(log.State, Is.EqualTo(TradeState.New));
        }

        [TestCase(0)]
        [TestCase(-5)]
        public void Should_refuse_to_create_a_trade_without_a_positive_quantity(double quantity)
        {
            var trade = new Trade();

            var result = trade.Create(new TriggerContext(), Data(quantity));

            Assert.That(result.IsFailure(), Is.True);
            Assert.That(trade.TradeState, Is.EqualTo(TradeState.Undefined));
            Assert.That(trade.EventLogs, Is.Empty);
        }

        [Test]
        public void Should_refuse_to_create_a_trade_without_data()
        {
            Assert.That(new Trade().Create(new TriggerContext(), null).IsFailure(), Is.True);
        }

        [Test]
        public void Should_refuse_to_create_a_trade_twice()
        {
            var trade = new Trade();
            trade.Create(new TriggerContext(), Data());

            var result = trade.Create(new TriggerContext(), Data(200));

            Assert.That(result.IsFailure(), Is.True);
            Assert.That(trade.ExecutionQuantity, Is.EqualTo(100));
            Assert.That(trade.EventLogs.Count, Is.EqualTo(1));
        }

        [Test]
        public void Should_cancel_a_created_trade()
        {
            var trade = new Trade();
            trade.Create(new TriggerContext(), Data());

            var result = trade.Cancel(new TriggerContext());

            Assert.That(result.IsSuccess(), Is.True, result.ErrorMessage);
            Assert.That(trade.TradeState, Is.EqualTo(TradeState.Cancelled));
            Assert.That(trade.EventLogs.Last().Trigger, Is.EqualTo(TradeTrigger.Cancel));
        }

        [Test]
        public void Should_refuse_to_cancel_a_trade_that_is_not_created()
        {
            var trade = new Trade();

            Assert.That(trade.Cancel(new TriggerContext()).IsFailure(), Is.True);
            Assert.That(trade.EventLogs, Is.Empty);
        }

        [Test]
        public void Should_refuse_to_cancel_a_trade_twice()
        {
            var trade = new Trade();
            trade.Create(new TriggerContext(), Data());
            trade.Cancel(new TriggerContext());

            Assert.That(trade.Cancel(new TriggerContext()).IsFailure(), Is.True);
            Assert.That(trade.EventLogs.Count, Is.EqualTo(2));
        }

        private static Trade CreatedTrade()
        {
            var trade = new Trade();
            trade.Create(new TriggerContext(), Data());
            return trade;
        }

        private static Trade SentTrade()
        {
            var trade = CreatedTrade();
            trade.BookingSend(new TriggerContext());
            return trade;
        }

        [Test]
        public void Should_send_a_new_trade_to_booking_and_confirm_it()
        {
            var trade = CreatedTrade();

            Assert.That(trade.BookingSend(new TriggerContext()).IsSuccess(), Is.True);
            Assert.That(trade.TradeState, Is.EqualTo(TradeState.BookingSent));
            Assert.That(trade.Booked(new TriggerContext()).IsSuccess(), Is.True);
            Assert.That(trade.TradeState, Is.EqualTo(TradeState.Booked));
            Assert.That(trade.EventLogs.Select(l => l.Trigger),
                Is.EqualTo(new[] { TradeTrigger.Create, TradeTrigger.BookingSend, TradeTrigger.Booked }));
        }

        [Test]
        public void Should_send_a_failed_trade_again()
        {
            var trade = SentTrade();

            Assert.That(trade.BookingFailed(new TriggerContext()).IsSuccess(), Is.True);
            Assert.That(trade.TradeState, Is.EqualTo(TradeState.BookingError));
            Assert.That(trade.BookingSend(new TriggerContext()).IsSuccess(), Is.True);
            Assert.That(trade.TradeState, Is.EqualTo(TradeState.BookingSent));
        }

        [Test]
        public void Should_keep_a_sent_trade_sent_when_refreshed()
        {
            var trade = SentTrade();

            Assert.That(trade.BookingRefresh(new TriggerContext()).IsSuccess(), Is.True);
            Assert.That(trade.TradeState, Is.EqualTo(TradeState.BookingSent));
            Assert.That(trade.EventLogs.Last().Trigger, Is.EqualTo(TradeTrigger.BookingRefresh));
        }

        [Test]
        public void Should_force_a_new_or_failed_trade_to_booked()
        {
            var fromNew = CreatedTrade();
            var fromError = SentTrade();
            fromError.BookingFailed(new TriggerContext());

            Assert.That(fromNew.ForceBooked(new TriggerContext()).IsSuccess(), Is.True);
            Assert.That(fromError.ForceBooked(new TriggerContext()).IsSuccess(), Is.True);
            Assert.That(fromNew.TradeState, Is.EqualTo(TradeState.Booked));
            Assert.That(fromError.TradeState, Is.EqualTo(TradeState.Booked));
        }

        [Test]
        public void Should_refuse_booking_steps_out_of_order()
        {
            var created = CreatedTrade();
            var sent = SentTrade();
            var booked = SentTrade();
            booked.Booked(new TriggerContext());

            Assert.That(created.Booked(new TriggerContext()).IsFailure(), Is.True);
            Assert.That(created.BookingFailed(new TriggerContext()).IsFailure(), Is.True);
            Assert.That(created.BookingRefresh(new TriggerContext()).IsFailure(), Is.True);
            Assert.That(sent.BookingSend(new TriggerContext()).IsFailure(), Is.True);
            Assert.That(sent.ForceBooked(new TriggerContext()).IsFailure(), Is.True);
            Assert.That(booked.BookingSend(new TriggerContext()).IsFailure(), Is.True);
            Assert.That(booked.BookingFailed(new TriggerContext()).IsFailure(), Is.True);
            Assert.That(new Trade().BookingSend(new TriggerContext()).IsFailure(), Is.True);
        }

        [Test]
        public void Should_not_log_a_refused_booking_step()
        {
            var trade = CreatedTrade();

            trade.Booked(new TriggerContext());

            Assert.That(trade.EventLogs.Count, Is.EqualTo(1));
            Assert.That(trade.TradeState, Is.EqualTo(TradeState.New));
        }

        [Test]
        public void Should_refuse_booking_a_cancelled_trade()
        {
            var trade = CreatedTrade();
            trade.Cancel(new TriggerContext());

            Assert.That(trade.BookingSend(new TriggerContext()).IsFailure(), Is.True);
            Assert.That(trade.ForceBooked(new TriggerContext()).IsFailure(), Is.True);
        }

        [Test]
        public void Should_cancel_a_trade_at_any_point_of_its_booking()
        {
            var sent = SentTrade();
            var booked = SentTrade();
            booked.Booked(new TriggerContext());

            Assert.That(sent.Cancel(new TriggerContext()).IsSuccess(), Is.True);
            Assert.That(booked.Cancel(new TriggerContext()).IsSuccess(), Is.True);
        }
    }
}
