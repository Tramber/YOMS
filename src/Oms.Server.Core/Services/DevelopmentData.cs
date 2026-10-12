using System;
using System.Linq;
using Oms.Server.Domain.Interfaces.Repository;
using Oms.Server.Domain.Models.Funds;
using Oms.Server.Domain.Models.Securities;
using Oms.Server.Domain.Models.Users;

namespace Oms.Server.Core.Services
{
    /// <summary>Demo reference data, so that a new database has a user to log in with and something to trade.</summary>
    public static class DevelopmentData
    {
        public const string Login = "trader";

        /// <summary>Adds a user (login <see cref="Login"/>), two funds and three securities when the database has no active user. Returns true when it did.</summary>
        public static bool SeedIfEmpty(IRepositoryFacade repositories)
        {
            if (repositories.Users.GetUserList().Any())
                return false;

            var trader = new User { FirstName = "Demo", LastName = "Trader", Login = Login, IsActive = true };
            repositories.Users.Add(trader);

            repositories.Funds.Add(new Fund { Name = "Alpha Fund", Portfolio = "ALPHA", Depositary = "Demo Bank" });
            repositories.Funds.Add(new Fund { Name = "Beta Fund", Portfolio = "BETA", Depositary = "Demo Bank" });

            repositories.Securities.Add(Security("ACME Corp", "FR0000000001", "ACME FP", Currency.Eur, SecurityType.Equity, trader));
            repositories.Securities.Add(Security("Globex 4% 2030", "US0000000002", "GLBX 4 30", Currency.Usd, SecurityType.Bond, trader));
            repositories.Securities.Add(Security("Initech Future", "GB0000000003", "INIT F", Currency.Gbp, SecurityType.Future, trader));
            return true;
        }

        private static Security Security(string name, string isin, string ticker, Currency currency, SecurityType type, User creator)
        {
            return new Security
            {
                Name = name,
                IsinCode = isin,
                BloombergTicker = ticker,
                Currency = currency,
                Type = type,
                IsActive = true,
                CreationDate = DateTime.Now,
                Creator = creator,
            };
        }
    }
}
