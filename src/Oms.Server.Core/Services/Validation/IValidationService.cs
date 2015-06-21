using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using log4net.Core;
using Oms.Server.Domain.Framework;
using Oms.Server.Domain.Interfaces.Models;

namespace Oms.Server.Core.Services.Validation
{
    interface IValidationService
    {
        GenericResult IsValid(IOrder order);
        GenericResult IsValid(IOrderCoreData orderCoreData);
        GenericResult IsValid(IOrderEditableData orderEditableData);
        GenericResult IsValid(ISecurity security);
    }
}
