using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Oms.Transport.Contracts;
using Oms.Transport.Contracts.Dto;
using Oms.Transport.Contracts.Messages;
using Oms.Transport.Contracts.Messages.Orders;

namespace Oms.Client.Services
{
    /// <summary>SignalR implementation of <see cref="IOrderServiceProxy"/>, talking to the server's <c>OrderHub</c>.</summary>
    public class OrderServiceProxy : IOrderServiceProxy
    {
        private readonly HubConnection _connection;

        public OrderServiceProxy(string serverUrl = "http://localhost:9001")
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(serverUrl + OrderHubMethods.Path)
                .WithAutomaticReconnect()
                .Build();
        }

        public bool IsConnected
        {
            get { return _connection.State == HubConnectionState.Connected; }
        }

        public async Task ConnectAsync()
        {
            if (_connection.State == HubConnectionState.Disconnected)
                await _connection.StartAsync();
            await _connection.InvokeAsync(OrderHubMethods.Subscribe);
        }

        public Task<List<OrderDto>> GetOrdersAsync()
        {
            return _connection.InvokeAsync<List<OrderDto>>(OrderHubMethods.GetOrders);
        }

        public Task<List<SecurityDto>> GetSecuritiesAsync()
        {
            return _connection.InvokeAsync<List<SecurityDto>>(OrderHubMethods.GetSecurities);
        }

        public Task<List<FundDto>> GetFundsAsync()
        {
            return _connection.InvokeAsync<List<FundDto>>(OrderHubMethods.GetFunds);
        }

        public Task<Response<Tuple<OrderDto, GenericResultDto>, EditionCommand>> SendAsync(OrderEditionRequest request)
        {
            return _connection.InvokeAsync<Response<Tuple<OrderDto, GenericResultDto>, EditionCommand>>(OrderHubMethods.HandleOrderEdition, request);
        }

        public Task<Response<Tuple<int, GenericResultDto>, OrderStateCommand>> SendAsync(OrderRequest request)
        {
            return _connection.InvokeAsync<Response<Tuple<int, GenericResultDto>, OrderStateCommand>>(OrderHubMethods.HandleOrder, request);
        }

        public Task<Response<Tuple<int, GenericResultDto>, OrderMarketCommand>> SendAsync(OrderMarketRequest request)
        {
            return _connection.InvokeAsync<Response<Tuple<int, GenericResultDto>, OrderMarketCommand>>(OrderHubMethods.HandleOrderMarket, request);
        }

        public Task<Response<Tuple<int, GenericResultDto>, OrderTradeCommand>> SendAsync(OrderTradeRequest request)
        {
            return _connection.InvokeAsync<Response<Tuple<int, GenericResultDto>, OrderTradeCommand>>(OrderHubMethods.HandleOrderTrade, request);
        }

        /// <summary>Calls the observer for each batch of orders pushed by the server. Dispose the result to stop.</summary>
        public IDisposable Subscribe(IObserver<Tuple<ItemOperationType, OrderDto[]>> observer)
        {
            return _connection.On<ItemOperationType, OrderDto[]>(OrderHubMethods.OrderListUpdated,
                (type, orders) => observer.OnNext(Tuple.Create(type, orders)));
        }

        public ValueTask DisposeAsync()
        {
            return _connection.DisposeAsync();
        }
    }
}
