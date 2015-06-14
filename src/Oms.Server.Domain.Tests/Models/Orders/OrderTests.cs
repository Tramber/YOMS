using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Oms.Server.Domain.Models.Funds;
using Oms.Server.Domain.Models.Instruments;
using Oms.Server.Domain.Models.Orders;
using Oms.Server.Domain.Models.Users;

namespace Oms.Server.Domain.Tests.Models.Orders
{
    [TestFixture]
    public class OrderTests
    {
        public void Should_allow_create_a_new_order()
        {
            var order = new OrderBuilder()
                .WithCoreData(new User(), new User(), new OrderBasket(), "nnn", "truc")
                .WithFund(new Fund())
                .WithInstrument(new Instrument())
                .WithInitialReferentialData("instCode", InstrumentCodeType.Isin, "fundCode", FundCodeType.Other,
                    "folioCode")
                .Build();

        }
    }
}
