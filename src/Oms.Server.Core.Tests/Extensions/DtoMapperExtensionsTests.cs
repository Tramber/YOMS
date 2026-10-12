using System;
using NUnit.Framework;
using Oms.Server.Core.Extensions;
using Oms.Server.Domain.Framework;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.EventLogs;
using Oms.Server.Domain.Models.Funds;
using Oms.Server.Domain;
using Oms.Server.Domain.Models.Orders;
using Oms.Server.Domain.Models.Securities;
using Oms.Server.Domain.Models.Users;
using Oms.Transport.Contracts.Dto;

namespace Oms.Server.Core.Tests.Extensions
{
    [TestFixture]
    public class DtoMapperExtensionsTests
    {
        [TestCase(OrderType.Market, OrderDtoType.Market)]
        [TestCase(OrderType.Limit, OrderDtoType.Limit)]
        [TestCase(OrderType.Stop, OrderDtoType.Stop)]
        public void Should_map_an_enum_to_the_dto_enum_with_the_same_name(OrderType origin, OrderDtoType expected)
        {
            Assert.That(origin.ToMappingEnum<OrderDtoType>(), Is.EqualTo(expected));
        }

        [Test]
        public void Should_refuse_to_map_an_enum_value_that_has_no_dto_equivalent()
        {
            // "Buy" is a side, the order types are Market, Limit and Stop.
            Assert.Throws<NotSupportedException>(() => Side.Buy.ToMappingEnum<OrderDtoType>());
        }

        [Test]
        public void Should_map_a_security_without_currency_or_type_instead_of_failing()
        {
            var dto = new Security { Id = 3 }.ToDto();

            Assert.That(dto.Currency, Is.EqualTo(CurrencyDto.Undefined));
            Assert.That(dto.Type, Is.EqualTo(SecurityDtoType.Undefined));
        }

        [Test]
        public void Should_map_a_user()
        {
            var user = new User { Id = 7, FirstName = "Ada", LastName = "Lovelace", Login = "ada" };

            var dto = user.ToDto();

            Assert.That(dto.Id, Is.EqualTo(7));
            Assert.That(dto.FirstName, Is.EqualTo("Ada"));
            Assert.That(dto.LastName, Is.EqualTo("Lovelace"));
            Assert.That(dto.Login, Is.EqualTo("ada"));
        }

        [Test]
        public void Should_map_a_security_with_its_creator_currency_and_type()
        {
            var created = new DateTime(2020, 1, 2);
            var security = new Security
            {
                Id = 3,
                Name = "ACME",
                IsinCode = "FR0000000001",
                BloombergTicker = "ACME FP",
                Currency = Currency.Eur,
                Type = SecurityType.Bond,
                IsActive = true,
                CreationDate = created,
                Creator = new User { Id = 9, Login = "creator" },
            };

            var dto = security.ToDto();

            Assert.That(dto.Id, Is.EqualTo(3));
            Assert.That(dto.Name, Is.EqualTo("ACME"));
            Assert.That(dto.IsinCode, Is.EqualTo("FR0000000001"));
            Assert.That(dto.BloombergTicker, Is.EqualTo("ACME FP"));
            Assert.That(dto.Currency, Is.EqualTo(CurrencyDto.Eur));
            Assert.That(dto.Type, Is.EqualTo(SecurityDtoType.Bond));
            Assert.That(dto.IsActive, Is.True);
            Assert.That(dto.CreationDate, Is.EqualTo(created));
            Assert.That(dto.Creator.Login, Is.EqualTo("creator"));
        }

        [Test]
        public void Should_map_an_order()
        {
            var order = new Order { Id = 5 };

            var dto = order.ToDto();

            Assert.That(dto.Id, Is.EqualTo(5));
            Assert.That(dto.OrderType, Is.EqualTo(OrderDtoType.Market));
        }

        [Test]
        public void Should_map_a_fund()
        {
            var dto = new Fund { Id = 4, Name = "Alpha", Portfolio = "P1", Depositary = "Bank" }.ToDto();

            Assert.That(dto.Id, Is.EqualTo(4));
            Assert.That(dto.Name, Is.EqualTo("Alpha"));
            Assert.That(dto.Portfolio, Is.EqualTo("P1"));
            Assert.That(dto.Depositary, Is.EqualTo("Bank"));
        }

        [Test]
        public void Should_map_a_result_with_its_error()
        {
            var failure = GenericResult.Failure("refused", 7).ToDto();
            var success = GenericResult.Success().ToDto();

            Assert.That(failure.ErrorMessage, Is.EqualTo("refused"));
            Assert.That(failure.ErrorCode, Is.EqualTo(7));
            Assert.That(success.ErrorMessage, Is.Null);
            Assert.That(success.ErrorCode, Is.EqualTo(GenericResult.NoErrorCode));
        }

        [Test]
        public void Should_map_the_data_of_an_order_that_was_sent()
        {
            var user = new User { Id = 2, Login = "trader" };
            var builder = new OrderBuilder()
                .WithCoreData(user, user, new OrderBasket(), "client-ref")
                .WithFund(new Fund { Id = 6, Name = "Alpha" })
                .WithSecurity(new Security { Id = 8, Name = "ACME", Currency = Currency.Eur, Type = SecurityType.Equity })
                .WithPrices(OrderType.Limit, 100, 12.5, 11)
                .WithDates(new DateTime(2030, 1, 3), OrderValidityType.Gtd, new DateTime(2030, 2, 1))
                .WithInitialReferentialData("instCode", SecurityCodeType.Isin, "fundCode", FundCodeType.Other, "folioCode");
            var order = new Order { Id = 5 };
            order.SendRequest(new TriggerContext(user), builder.BuildOrderCoreData(), builder.BuildOrderEditableData());

            var dto = order.ToDto();

            Assert.That(dto.Id, Is.EqualTo(5));
            Assert.That(dto.Quantity, Is.EqualTo(100));
            Assert.That(dto.PriceLimit, Is.EqualTo(12.5));
            Assert.That(dto.PriceStop, Is.EqualTo(11));
            Assert.That(dto.Way, Is.EqualTo(OrderDtoWay.Buy));
            Assert.That(dto.OrderType, Is.EqualTo(OrderDtoType.Limit));
            Assert.That(dto.Validity, Is.EqualTo(OrderDtoValidityType.Gtd));
            Assert.That(dto.SettlementDate, Is.EqualTo(new DateTime(2030, 1, 3)));
            Assert.That(dto.ExpiryDate, Is.EqualTo(new DateTime(2030, 2, 1)));
            Assert.That(dto.ClientOrderRef, Is.EqualTo("client-ref"));
            Assert.That(dto.Owner.Login, Is.EqualTo("trader"));
            Assert.That(dto.Creator.Login, Is.EqualTo("trader"));
            Assert.That(dto.Security.Name, Is.EqualTo("ACME"));
            Assert.That(dto.Fund.Name, Is.EqualTo("Alpha"));
        }

        [Test]
        public void Should_map_the_state_status_and_remaining_quantity_of_an_order()
        {
            var user = new User { Id = 2, Login = "trader" };
            var builder = new OrderBuilder()
                .WithCoreData(user, user, new OrderBasket(), "ref")
                .WithFund(new Fund())
                .WithSecurity(new Security { Currency = Currency.Eur, Type = SecurityType.Equity })
                .WithPrices(OrderType.Limit, 100, 10, null)
                .WithInitialReferentialData("instCode", SecurityCodeType.Isin, "fundCode", FundCodeType.Other, "folioCode");
            var order = new Order { Id = 5 };
            order.SendRequest(new TriggerContext(user), builder.BuildOrderCoreData(), builder.BuildOrderEditableData());
            order.Cancel(new TriggerContext(user));

            var dto = order.ToDto();

            Assert.That(dto.State, Is.EqualTo("Accepting"));
            Assert.That(dto.Status, Is.EqualTo("CancelPending"));
            Assert.That(dto.RemainingQuantity, Is.EqualTo(100));
        }
    }
}
