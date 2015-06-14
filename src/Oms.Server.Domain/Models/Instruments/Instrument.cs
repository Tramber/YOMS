using System;
using Oms.Server.Domain.Interfaces;
using Oms.Server.Domain.Interfaces.Repository;
using Oms.Server.Domain.Models.Users;

namespace Oms.Server.Domain.Models.Instruments
{
    public class Instrument : IIdentifiable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string IsinCode { get; set; }
        public string BloombergTicker { get; set; }
        public Currency Currency { get; set; }
        public InstrumentType Type { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreationDate { get; set; }
        public User Creator { get; set; }
    }
}
