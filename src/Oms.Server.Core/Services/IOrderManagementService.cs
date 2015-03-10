using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Oms.Transport.Contracts;
using Oms.Transport.Contracts.Dto;

namespace Oms.Server.Core.Services
{
    public interface IOrderManagementService
    {
        Task<ItemOperationResponse<OrderDto>> CreateOrders(List<OrderDto> orderDtoList);
        Task<ItemOperationResponse<OrderDto>> CancelOrders(List<int> orderIdList);
        Task<ItemOperationResponse<OrderDto>> UpdateOrders(List<OrderDto> orderDtoList);
    }

}