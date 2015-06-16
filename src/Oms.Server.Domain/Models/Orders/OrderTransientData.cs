using System;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.Funds;
using Oms.Server.Domain.Models.Instruments;

namespace Oms.Server.Domain.Models.Orders
{
    public class OrderTransientData : IOrderTransientData
    {
        public double Quantity { get; set; }

        public double Price { get; set; }

        public Side Side { get; set; }

        public Instrument Instrument { get; set; }

        public OrderType OrderType { get; set; }

        public OrderValidityType OrderValidity { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public Fund Fund { get; set; }
    }
}