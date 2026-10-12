using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Oms.Server.DataAccess.NHibernate.Repositories;
using Oms.Server.Domain;
using Oms.Server.Domain.Framework;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.EventLogs;
using Oms.Server.Domain.Models.Funds;
using Oms.Server.Domain.Models.Orders;
using Oms.Server.Domain.Models.Securities;
using Oms.Server.Domain.Models.Users;
using Oms.Server.Domain.Workflow;

namespace Oms.Server.DataAccess.Tests.Repositories
{
    [TestFixture]
    public class OrderRepositoryTests
    {
        private static Order SavedAcceptingOrder(RepositoryFacade facade, string suffix, out User user)
        {
            user = new User { Login = "creator-" + suffix, IsActive = true };
            var fund = new Fund { Name = "Fund " + suffix };
            var security = new Security { Name = "Security " + suffix, Creator = user, BloombergTicker = "T", CreationDate = DateTime.Now };
            facade.Users.Add(user);
            facade.Funds.Add(fund);
            facade.Securities.Add(security);

            var builder = new OrderBuilder()
                .WithCoreData(user, user, new OrderBasket(), "ref-" + suffix)
                .WithFund(fund)
                .WithSecurity(security)
                .WithPrices(OrderType.Limit, 100, 12.5, null)
                .WithInitialReferentialData("instCode", SecurityCodeType.Isin, "fundCode", FundCodeType.Other, "folio");
            var order = new Order();
            var result = order.SendRequest(new TriggerContext(user), builder.BuildOrderCoreData(), builder.BuildOrderEditableData());
            Assert.That(result.ErrorMessage, Is.Null);
            facade.Orders.Add(order);
            return order;
        }

        [Test]
        public void Should_list_every_saved_order_with_its_event_log()
        {
            var facade = new RepositoryFacade();
            User user;
            var first = SavedAcceptingOrder(facade, Guid.NewGuid().ToString("N"), out user);
            var second = SavedAcceptingOrder(facade, Guid.NewGuid().ToString("N"), out user);

            var orders = facade.Orders.GetOrderList();

            var listedFirst = orders.Single(o => o.Id == first.Id);
            var listedSecond = orders.Single(o => o.Id == second.Id);
            Assert.That(listedFirst.EventLogs.Count, Is.EqualTo(1));
            Assert.That(listedSecond.ClientOrderRef, Is.EqualTo(second.ClientOrderRef));
        }

        [Test]
        public void Should_save_the_new_event_log_entries_when_the_order_is_updated()
        {
            var facade = new RepositoryFacade();
            User user;
            var order = SavedAcceptingOrder(facade, Guid.NewGuid().ToString("N"), out user);

            Assert.That(order.SendAccept(new TriggerContext(user, DateTime.Now.AddMinutes(1))).IsSuccess(), Is.True);
            facade.Orders.Update(order);

            var loaded = facade.Orders.GetOrderById(order.Id);
            Assert.That(loaded.EventLogs.Count, Is.EqualTo(2));
            Assert.That(loaded.OrderState, Is.EqualTo(OrderStateMachine.State.Working));
        }

        [Test]
        public void Should_save_an_order_with_its_event_log_and_load_it_back()
        {
            var suffix = Guid.NewGuid().ToString("N");
            var user = new User { Login = "creator-" + suffix, IsActive = true };
            var fund = new Fund { Name = "Fund " + suffix };
            var security = new Security { Name = "Security " + suffix, Creator = user, BloombergTicker = "T", CreationDate = DateTime.Now };
            var facade = new RepositoryFacade();
            facade.Users.Add(user);
            facade.Funds.Add(fund);
            facade.Securities.Add(security);

            var builder = new OrderBuilder()
                .WithCoreData(user, user, new OrderBasket(), "ref-" + suffix)
                .WithFund(fund)
                .WithSecurity(security)
                .WithPrices(OrderType.Limit, 100, 12.5, null)
                .WithInitialReferentialData("instCode", SecurityCodeType.Isin, "fundCode", FundCodeType.Other, "folio");
            var order = new Order();
            var result = order.SendRequest(new TriggerContext(user), builder.BuildOrderCoreData(), builder.BuildOrderEditableData());
            Assert.That(result.ErrorMessage, Is.Null);

            facade.Orders.Add(order);

            Assert.That(order.Id, Is.GreaterThan(0));
            var loaded = facade.Orders.GetOrderById(order.Id);
            Assert.That(loaded.ClientOrderRef, Is.EqualTo("ref-" + suffix));
            Assert.That(loaded.EventLogs.Count, Is.EqualTo(1));
            Assert.That(loaded.OrderState, Is.EqualTo(OrderStateMachine.State.Accepting));
            Assert.That(loaded.Quantity, Is.EqualTo(100));
            Assert.That(loaded.Creator.Login, Is.EqualTo(user.Login));
        }
    }
}
