using System;

namespace Oms.Server.Domain.Interfaces.Models
{
    public interface IOrderCoreData
    {
        IUser Owner { get; }

        IUser Creator { get; }

        DateTime CreationDate { get; }

        IOrderBasket OrderBasket { get; }

        IOrderInitialReferentialData InitialReferentialData { get; }

        string ClientOrderRef { get; }

        bool IsAutoAccepted { get; }
    }
}
