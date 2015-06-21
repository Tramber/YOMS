using Oms.Server.Domain.DomainEvents;
using Oms.Server.Domain.Framework;
using Oms.Server.Domain.Interfaces.Models;

namespace Oms.Server.Core.Services.Acceptance
{
    public class AcceptanceService : IAcceptanceService
    {
        public GenericResult IsAutoAccepted(IOrder order)
        {
            return GenericResult.Success();
        }
        public void Handle(ValidationDomainEvent args)
        {
            throw new System.NotImplementedException();
        }
    }
}