using System;
using System.Collections.Generic;
using NUnit.Framework;
using Oms.Client.Model;
using Oms.Client.Services;
using Oms.Transport.Contracts.Dto;

namespace Oms.Client.Tests.Model
{
    [TestFixture]
    public class OrderAdapterTests
    {
        private static OrderDto Dto()
        {
            return new OrderDto
            {
                Id = 3,
                ClientOrderRef = "ref",
                Quantity = 100,
                RemainingQuantity = 40,
                PriceLimit = 9.5,
                Way = OrderDtoWay.Sell,
                OrderType = OrderDtoType.Limit,
                Validity = OrderDtoValidityType.Gtc,
                ExpiryDate = new DateTime(2030, 1, 1),
                CreationDate = new DateTime(2029, 5, 5),
                State = "Dealing",
                Status = "PartiallyFilled",
                Owner = new UserDto { Login = "owner" },
                Creator = new UserDto { Login = "creator" },
                Security = new SecurityDto { Id = 1, Name = "ACME", IsinCode = "FR0000000001" },
            };
        }

        [Test]
        public void Should_take_every_value_of_the_dto()
        {
            var adapter = OrderAdapter.From(Dto());

            Assert.That(adapter.Id, Is.EqualTo(3));
            Assert.That(adapter.ClientOrderRef, Is.EqualTo("ref"));
            Assert.That(adapter.Quantity, Is.EqualTo(100));
            Assert.That(adapter.RemainingQuantity, Is.EqualTo(40));
            Assert.That(adapter.Price, Is.EqualTo(9.5));
            Assert.That(adapter.Way, Is.EqualTo(OrderDtoWay.Sell));
            Assert.That(adapter.OrderType, Is.EqualTo(OrderDtoType.Limit));
            Assert.That(adapter.Validity, Is.EqualTo(OrderDtoValidityType.Gtc));
            Assert.That(adapter.ExpiryDate, Is.EqualTo(new DateTime(2030, 1, 1)));
            Assert.That(adapter.CreationDate, Is.EqualTo(new DateTime(2029, 5, 5)));
            Assert.That(adapter.State, Is.EqualTo("Dealing"));
            Assert.That(adapter.Status, Is.EqualTo("PartiallyFilled"));
            Assert.That(adapter.Owner.Login, Is.EqualTo("owner"));
            Assert.That(adapter.Creator.Login, Is.EqualTo("creator"));
            Assert.That(adapter.Security.IsinCode, Is.EqualTo("FR0000000001"));
            Assert.That(adapter.SecurityName, Is.EqualTo("ACME"));
        }

        [Test]
        public void Should_show_no_price_for_an_order_without_limit_and_no_name_without_security()
        {
            var dto = Dto();
            dto.PriceLimit = null;
            dto.Security = null;

            var adapter = OrderAdapter.From(dto);

            Assert.That(adapter.Price, Is.EqualTo(0));
            Assert.That(adapter.SecurityName, Is.Null);
        }

        [Test]
        public void Should_keep_its_local_group_and_tell_the_view_what_changed_when_updated()
        {
            var adapter = OrderAdapter.From(Dto());
            adapter.GroupId = 4001;
            var changed = new List<string>();
            adapter.PropertyChanged += (_, e) => changed.Add(e.PropertyName);
            var update = Dto();
            update.State = "Terminated";
            update.Status = "CancelledFilled";
            update.Security = new SecurityDto { Id = 2, Name = "Globex" };

            adapter.UpdateFrom(update);

            Assert.That(adapter.GroupId, Is.EqualTo(4001));
            Assert.That(adapter.State, Is.EqualTo("Terminated"));
            Assert.That(adapter.SecurityName, Is.EqualTo("Globex"));
            Assert.That(changed, Does.Contain("State").And.Contain("Status").And.Contain("SecurityName"));
        }

        [Test]
        public void Should_not_notify_for_a_value_that_did_not_change()
        {
            var adapter = OrderAdapter.From(Dto());
            var changed = new List<string>();
            adapter.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            adapter.UpdateFrom(Dto());

            Assert.That(changed, Does.Not.Contain("State"));
            Assert.That(changed, Does.Not.Contain("Quantity"));
        }
    }

    [TestFixture]
    public class SessionTests
    {
        [Test]
        public void Should_default_to_the_demo_login_and_notify_when_it_changes()
        {
            var session = new Session();
            var changed = new List<string>();
            session.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            Assert.That(session.Login, Is.EqualTo("trader"));
            session.Login = "alice";

            Assert.That(changed, Is.EqualTo(["Login"]));
        }
    }
}
