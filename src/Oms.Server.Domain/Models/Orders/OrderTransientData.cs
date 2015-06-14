using System;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.Instruments;

namespace Oms.Server.Domain.Models.Orders
{
    public class OrderTransientData : IOrderTransientData
    {
        public double Quantity { get; internal set; }

        public double Price { get; internal set; }

        public OrderWay Way { get; internal set; }

        public Instrument Instrument { get; internal set; }

        public OrderType OrderType { get; internal set; }

        public OrderValidityType OrderValidity { get; internal set; }

        public DateTime? ExpiryDate { get; internal set; }
    }
}