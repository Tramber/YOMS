using System;
using Oms.Server.Domain.Interfaces.Repository;
using Oms.Server.Domain.Models.Securities;

namespace Oms.Server.Domain.Interfaces.Models
{
    public interface ISecurity : IIdentifiable
    {
        string Name { get; set; }
        string IsinCode { get; set; }
        string BloombergTicker { get; set; }
        Currency Currency { get; set; }
        SecurityType Type { get; set; }
        bool IsActive { get; set; }
        DateTime CreationDate { get; set; }
        IUser Creator { get; set; }
    }
}