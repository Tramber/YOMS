using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Oms.Transport.Contracts;
using Oms.Transport.Contracts.Dto;
using Oms.Transport.Contracts.Messages;
using Oms.Transport.Contracts.Messages.Orders;

/// <summary>SignalR entry point of the server, mapped at <see cref="OrderHubMethods.Path"/>. Forwards requests to the order service and pushes order changes to the subscribers.</summary>
public class OrderHub : Hub
{
    public const string SubscribersGroup = "order-subscribers";
    private readonly IOrderServiceContract _service;

    public OrderHub(IOrderServiceContract service)
    {
        _service = service;
    }

    // SignalR hub methods cannot be overloaded, hence one name per request type.
    public EditionResponse<OrderDto> HandleOrderEdition(OrderEditionRequest request) => _service.Handle(request);

    public Response<Tuple<int, GenericResultDto>, OrderStateCommand> HandleOrder(OrderRequest request) => _service.Handle(request);

    public Response<Tuple<int, GenericResultDto>, OrderStateCommand> HandleOrderMarket(OrderMarketRequest request) => _service.Handle(request);

    public Task SubscribeToEvents() => Groups.AddToGroupAsync(Context.ConnectionId, SubscribersGroup);

    public Task UnsubscribeToEvents() => Groups.RemoveFromGroupAsync(Context.ConnectionId, SubscribersGroup);
}
