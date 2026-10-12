using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Oms.Server.Core.Services;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Interfaces.Repository;
using Oms.Server.Domain.Models.Funds;
using Oms.Server.Domain.Models.Securities;
using Oms.Server.Domain.Models.Users;

namespace Oms.Server.Core.Tests.Services
{
    [TestFixture]
    public class DevelopmentDataTests
    {
        private IRepositoryFacade _repositories;
        private IUserRepository _users;
        private IFundRepository _funds;
        private ISecurityRepository _securities;

        [SetUp]
        public void SetUp()
        {
            _users = Substitute.For<IUserRepository>();
            _funds = Substitute.For<IFundRepository>();
            _securities = Substitute.For<ISecurityRepository>();
            _repositories = Substitute.For<IRepositoryFacade>();
            _repositories.Users.Returns(_users);
            _repositories.Funds.Returns(_funds);
            _repositories.Securities.Returns(_securities);
        }

        [Test]
        public void Should_seed_a_user_funds_and_securities_into_an_empty_database()
        {
            _users.GetUserList().Returns(new List<User>());

            var seeded = DevelopmentData.SeedIfEmpty(_repositories);

            Assert.That(seeded, Is.True);
            _users.Received(1).Add(Arg.Is<User>(u => u.Login == DevelopmentData.Login && u.IsActive));
            _funds.Received(2).Add(Arg.Any<IFund>());
            _securities.Received(3).Add(Arg.Any<Security>());
        }

        [Test]
        public void Should_seed_securities_that_orders_can_be_placed_on()
        {
            _users.GetUserList().Returns(new List<User>());
            var securities = new List<Security>();
            _securities.When(s => s.Add(Arg.Any<Security>())).Do(call => securities.Add(call.Arg<Security>()));

            DevelopmentData.SeedIfEmpty(_repositories);

            Assert.That(securities.All(s => s.IsActive && s.Currency != Currency.Undefined && s.Type != SecurityType.Undefined && s.Creator != null), Is.True);
            Assert.That(securities.Select(s => s.IsinCode).Distinct().Count(), Is.EqualTo(3));
        }

        [Test]
        public void Should_leave_a_database_that_has_users_alone()
        {
            _users.GetUserList().Returns(new List<User> { new User { Login = "someone", IsActive = true } });

            var seeded = DevelopmentData.SeedIfEmpty(_repositories);

            Assert.That(seeded, Is.False);
            _users.DidNotReceive().Add(Arg.Any<User>());
            _funds.DidNotReceive().Add(Arg.Any<IFund>());
            _securities.DidNotReceive().Add(Arg.Any<Security>());
        }
    }
}
