using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Oms.Server.Domain.Models.Users;

namespace Oms.Server.Domain.Interfaces.Models
{
    public interface ITriggerContext
    {
        User User { get; }

        DateTime Date { get; }

        string Message { get; }

        string PlateformOrigin { get; } 
    }
}
