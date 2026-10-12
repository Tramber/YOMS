using Oms.Server.Domain.Framework;
using Oms.Server.Domain.Interfaces.Models;

namespace Oms.Server.Core.Services.Acceptance
{
    public interface IAcceptanceService
    {
        /// <summary>Success when the order is accepted as soon as it is sent (it then goes from Accepting to Working); a failure leaves it in Accepting.</summary>
        GenericResult IsAutoAccepted(IOrder order);
    }
}
