﻿using System;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.Instruments;

namespace Oms.Server.Domain.Models.Orders
{
    public partial class Order : IOrderTransientData, IOrderDealingData
    {
        public double Quantity
        {
            get { return CurrentData.Quantity; }
        }

        public double Price
        {
            get { return CurrentData.Price; }
        }

        public OrderWay Way
        {
            get { return CurrentData.Way; }
        }

        public Instrument Instrument
        {
            get { return CurrentData.Instrument; }
        }

        public OrderType OrderType
        {
            get { return CurrentData.OrderType; }
        }

        public OrderValidityType OrderValidity
        {
            get { return CurrentData.OrderValidity; }
        }

        public DateTime? ExpiryDate
        {
            get { return CurrentData.ExpiryDate; }
        }

        public double? RemainingQuantity
        {
            get { return CurrentData.RemainingQuantity; }
        }
    }
}