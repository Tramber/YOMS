using System;
using Oms.Server.Domain.Interfaces.Models;

namespace Oms.Server.Domain.Models.EventLogs
{
    /// <summary>Who fired a trigger, when and from which platform.</summary>
    public class TriggerContext : ITriggerContext
    {
        public TriggerContext()
        {
        }

        /// <summary>Context for the given user; the date defaults to now.</summary>
        public TriggerContext(IUser user, DateTime? date = null, string plateformOrigin = null)
        {
            User = user;
            Date = date ?? DateTime.Now;
            PlateformOrigin = plateformOrigin;
        }

        public IUser User { get; internal set; }
        public DateTime Date { get; internal set; }
        public string PlateformOrigin { get; internal set; }
    }
}