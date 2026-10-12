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
    }
}
