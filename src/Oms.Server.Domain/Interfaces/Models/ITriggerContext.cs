using System;

namespace Oms.Server.Domain.Interfaces.Models
{
    public interface ITriggerContext
    {
        IUser User { get; }

        DateTime Date { get; }

        string PlateformOrigin { get; } 
    }
}
