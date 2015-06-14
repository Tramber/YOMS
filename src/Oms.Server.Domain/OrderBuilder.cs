using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oms.Server.Domain.Models.Funds;
using Oms.Server.Domain.Models.Generics;
using Oms.Server.Domain.Models.Instruments;
using Oms.Server.Domain.Models.Orders;
using Oms.Server.Domain.Models.Users;

namespace Oms.Server.Domain
{
    public class OrderBuilder
    {
        private OrderTransientData transientData = new OrderTransientData();
        private OrderInitialReferentialData initialReferentialData;
        private OrderCoreData coreData;

        public OrderBuilder()
        {   
        }

        public OrderBuilder WithInstrument(Instrument instrument)
        {
            transientData.Instrument = instrument;
            return this;
        }

        public OrderBuilder WithFund(Fund fund)
        {
            transientData.Fund = fund;
            return this;
        }

        public OrderBuilder WithCoreData(User creator, User owner, OrderBasket orderBasket, string orderClientRef, string applicationOrigin)
        {
            coreData = new OrderCoreData()
            {
                Creator = creator,
                ApplicationOrigin = applicationOrigin,
                CreationDate = DateTime.Now,
                InitialOrderBasket = orderBasket,
                OrderClientRef = orderClientRef,
                OrderBasket = orderBasket,
                Owner = owner ?? creator
            };
            return this;
        }


        public OrderBuilder WithInitialReferentialData(
            string instrumentCode,
            InstrumentCodeType instrumentCodeType,
            string fundCode,
            FundCodeType fundCodeType,
            string fundFolioCode)
        {
            initialReferentialData = new OrderInitialReferentialData
            {
                FundCode = fundCode,
                FundCodeType = fundCodeType,
                FundFolioCode = fundFolioCode,
                InstrumentCode = instrumentCode,
                InstrumentCodeType = instrumentCodeType
            };
            return this;
        }

        public Order Build(bool isDraft = false)
        {
            var order = new Order();
            coreData.InitialReferentialData = initialReferentialData;
            var triggerContext = new TriggerContext
            {
                Date = coreData.CreationDate,
                User = coreData.Creator,
            };
            var result = isDraft
                ? order.Create(triggerContext, coreData, transientData)
                : order.SendRequest(triggerContext, coreData, transientData);
            if(result.IsFailure()) throw new ApplicationException(result.ErrorMessage);
            return order;
        }

    }
}
