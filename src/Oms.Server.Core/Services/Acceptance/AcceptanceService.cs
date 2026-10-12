using Oms.Server.Domain.Framework;
using Oms.Server.Domain.Interfaces.Models;

namespace Oms.Server.Core.Services.Acceptance
{
    /// <summary>Accepts every order. The place for real acceptance rules (limits, user rights, ...) when there are some.</summary>
    public class AcceptanceService : IAcceptanceService
    {
        public GenericResult IsAutoAccepted(IOrder order)
        {
            return GenericResult.Success();
        }
    }
}
