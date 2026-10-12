using Oms.Server.Domain.Framework;
using Oms.Server.Domain.Interfaces.Models;

namespace Oms.Server.Core.Services.Acceptance
{
    public interface IAcceptanceService
    {
        GenericResult IsAutoAccepted(IOrder order);
    }
}
