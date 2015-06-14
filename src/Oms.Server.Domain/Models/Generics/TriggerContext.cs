using System;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.Users;

namespace Oms.Server.Domain.Models.Generics
{
    internal class TriggerContext : ITriggerContext
    {
        public User User { get; internal set; }
        public DateTime Date { get; internal set; }
        public string Message { get; internal set; }
        public string PlateformOrigin { get; internal set; }
    }
}