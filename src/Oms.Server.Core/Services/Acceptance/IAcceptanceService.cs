using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oms.Server.Domain.DomainEvents;
using Oms.Server.Domain.Framework;
using Oms.Server.Domain.Interfaces.Models;

namespace Oms.Server.Core.Services.Acceptance
{
    public interface IAcceptanceService
    {
        GenericResult IsAutoAccepted(IOrder order);
    }
}
