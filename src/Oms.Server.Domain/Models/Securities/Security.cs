using System;
using Oms.Server.Domain.Interfaces.Models;

namespace Oms.Server.Domain.Models.Securities
{
    public class Security : ISecurity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string IsinCode { get; set; }
        public string BloombergTicker { get; set; }
        public Currency Currency { get; set; }
        public SecurityType Type { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreationDate { get; set; }
        public IUser Creator { get; set; }
        public string SophisCode { get; set; }
    }
}
