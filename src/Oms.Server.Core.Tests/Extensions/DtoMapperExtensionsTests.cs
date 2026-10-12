using System;
using NUnit.Framework;
using Oms.Server.Core.Extensions;
using Oms.Server.Domain.Interfaces.Models;
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
            Assert.Throws<NotSupportedException>(() => Currency.Undefined.ToMappingEnum<CurrencyDto>());
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
                Currency = Currency.EUR,
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
            Assert.That(dto.Currency, Is.EqualTo(CurrencyDto.EUR));
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
    }
}
