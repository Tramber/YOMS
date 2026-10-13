using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Oms.Server.Core.Extensions;
using Oms.Server.Core.Services;
using Oms.Server.Core.Services.Filtering;
using Oms.Server.Domain.Framework;
using Oms.Transport.Contracts;
using Oms.Transport.Contracts.Dto;
using Oms.Transport.Contracts.Messages;
using Oms.Transport.Contracts.Messages.Orders;

namespace Oms.Server.Core.Contracts
{
    /// <summary>
    /// Server implementation of the order service: runs the request through <see cref="IOrderManagementService"/>, answers
    /// with one outcome per order and raises <see cref="OrderListUpdated"/> for the orders that changed.
    /// </summary>
    public class OrderServiceContract(IOrderManagementService orderManagementService, IFilterInterpreter filterInterpreter = null) : IOrderServiceContract
    {
        /// <summary>Raised when orders change; the SignalR host forwards it to subscribed clients.</summary>
        public event Action<ItemOperationType, OrderDto[]> OrderListUpdated;

        public Response<Tuple<OrderDto, GenericResultDto>, EditionCommand> Handle(OrderEditionRequest request)
        {
            var results = orderManagementService.HandleRequest(request);
            Publish(request.Command == EditionCommand.Update ? ItemOperationType.Update : ItemOperationType.Add, results);
            // An order that failed is answered with the DTO that was submitted.
            var items = results
                .Select((r, i) => Tuple.Create(r.Result.IsSuccess() ? r.Order.ToDto() : request.ItemList[i], r.Result.ToDto()))
                .ToList();
            return new Response<Tuple<OrderDto, GenericResultDto>, EditionCommand>(request.Command, items);
        }

        public Response<Tuple<int, GenericResultDto>, OrderStateCommand> Handle(OrderRequest request)
        {
            var results = orderManagementService.HandleRequest(request);
            Publish(request.Command == OrderStateCommand.Delete ? ItemOperationType.Delete : ItemOperationType.Update, results);
            return new Response<Tuple<int, GenericResultDto>, OrderStateCommand>(request.Command, Outcomes(results));
        }

        public Response<Tuple<int, GenericResultDto>, OrderMarketCommand> Handle(OrderMarketRequest request)
        {
            var results = orderManagementService.HandleRequest(request);
            Publish(ItemOperationType.Update, results);
            return new Response<Tuple<int, GenericResultDto>, OrderMarketCommand>(request.Command, Outcomes(results));
        }

        public Response<Tuple<int, GenericResultDto>, OrderTradeCommand> Handle(OrderTradeRequest request)
        {
            var results = orderManagementService.HandleRequest(request);
            Publish(ItemOperationType.Update, results);
            return new Response<Tuple<int, GenericResultDto>, OrderTradeCommand>(request.Command, Outcomes(results));
        }

        public List<OrderDto> GetOrders()
        {
            return orderManagementService.GetOrders().Select(o => o.ToDto()).ToList();
        }

        public List<SecurityDto> GetSecurities()
        {
            return orderManagementService.GetSecurities().Select(s => s.ToDto()).ToList();
        }

        public List<FundDto> GetFunds()
        {
            return orderManagementService.GetFunds().Select(f => f.ToDto()).ToList();
        }

        public Task<OrderFilterDto> InterpretFilter(string text)
        {
            return filterInterpreter == null ? Task.FromResult<OrderFilterDto>(null) : filterInterpreter.InterpretAsync(text, GetFunds(), GetSecurities());
        }

        private static List<Tuple<int, GenericResultDto>> Outcomes(IEnumerable<OrderCommandResult> results)
        {
            return results.Select(r => Tuple.Create(r.OrderId, r.Result.ToDto())).ToList();
        }

        /// <summary>Tells the subscribers about the orders that changed; nothing is sent when every order failed.</summary>
        private void Publish(ItemOperationType operationType, IEnumerable<OrderCommandResult> results)
        {
            var changed = results.Where(r => r.Result.IsSuccess()).Select(r => r.Order.ToDto()).ToArray();
            if (changed.Length > 0 && OrderListUpdated != null)
                OrderListUpdated(operationType, changed);
        }
    }
}
