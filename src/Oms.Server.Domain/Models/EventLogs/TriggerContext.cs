using System;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.Users;

namespace Oms.Server.Domain.Models.EventLogs
{
    public class TriggerContext : ITriggerContext
    {
        public TriggerContext()
        {
        }

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