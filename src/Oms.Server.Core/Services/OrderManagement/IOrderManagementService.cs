using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Oms.Server.Domain.Framework;
using Oms.Transport.Contracts;
using Oms.Transport.Contracts.Dto;
using Oms.Transport.Contracts.Messages;
using Oms.Transport.Contracts.Messages.Orders;

namespace Oms.Server.Core.Services
{
    public interface IOrderManagementService
    {
        GenericResult HandleRequest(OrderEditionRequest request);
        GenericResult HandleRequest(OrderMarketRequest request);
        GenericResult HandleRequest(OrderRequest request);
    }
}