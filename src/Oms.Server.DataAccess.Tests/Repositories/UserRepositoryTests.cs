using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Oms.Server.DataAccess.NHibernate.Repositories;
using Oms.Server.Domain.Models.Users;

namespace Oms.Server.DataAccess.Tests.Repositories
{
    /// <summary>Round trip through NHibernate and a real SQLite file (the schema is created on first use).</summary>
    [TestFixture]
    public class UserRepositoryTests
    {
        [Test]
        public void Should_save_a_user_and_find_it_by_login_and_id()
        {
            var login = "user-" + Guid.NewGuid().ToString("N");
            var repository = new UserRepository();
            var user = new User { FirstName = "Ada", LastName = "Lovelace", Login = login, IsActive = true };

            repository.Add(user);

            Assert.That(user.Id, Is.GreaterThan(0));
            var byLogin = repository.GetUserByLogin(login);
            Assert.That(byLogin, Is.Not.Null);
            Assert.That(byLogin.FirstName, Is.EqualTo("Ada"));
            Assert.That(repository.GetUserById(user.Id).Login, Is.EqualTo(login));
        }

        [Test]
        public void Should_return_nothing_for_an_unknown_login()
        {
            Assert.That(new UserRepository().GetUserByLogin("nobody-" + Guid.NewGuid().ToString("N")), Is.Null);
        }

        [Test]
        public void Should_list_only_active_users()
        {
            var repository = new UserRepository();
            var suffix = Guid.NewGuid().ToString("N");
            var active = new User { Login = "active-" + suffix, IsActive = true };
            var inactive = new User { Login = "inactive-" + suffix, IsActive = false };
            repository.Add([active, inactive]);

            var logins = repository.GetUserList().Select(u => u.Login).ToList();

            Assert.That(logins, Does.Contain(active.Login));
            Assert.That(logins, Does.Not.Contain(inactive.Login));
        }
    }
}
