using System;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.Users;

namespace Oms.Server.Domain.Framework
{
    public class TriggerContext : ITriggerContext
    {
        public TriggerContext()
        {
        }

        public TriggerContext(User user, DateTime? date = null, string plateformOrigin = null, string message = null)
        {
            User = user;
            Date = date ?? DateTime.Now;
            Message = message;
            PlateformOrigin = plateformOrigin;
        }

        public User User { get; internal set; }
        public DateTime Date { get; internal set; }
        public string Message { get; internal set; }
        public string PlateformOrigin { get; internal set; }
    }
}