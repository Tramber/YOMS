using Oms.Server.Domain.Framework;
using Oms.Server.Domain.Interfaces.Models;

namespace Oms.Server.Domain.Validation
{
    interface IValidationService
    {
        GenericResult IsValid(IOrder order);
        GenericResult IsValid(IOrderCoreData orderCoreData);
        GenericResult IsValid(IOrderEditableData orderEditableData);
        GenericResult IsValid(ISecurity security);
    }
}
