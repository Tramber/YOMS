using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Oms.Transport.Contracts;
using Oms.Transport.Contracts.Dto;

namespace Oms.Client.Services
{
    public class OrderServiceProxy : IObservable<Tuple<ItemOperationType, OrderDto[]>>, IAsyncDisposable
    {
        private readonly HubConnection _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:9001" + OrderHubMethods.Path)
            .WithAutomaticReconnect()
            .Build();

        public async Task StartAsync()
        {
            await _connection.StartAsync();
            await _connection.InvokeAsync(OrderHubMethods.Subscribe);
        }

        public IDisposable Subscribe(IObserver<Tuple<ItemOperationType, OrderDto[]>> observer)
        {
            return _connection.On<ItemOperationType, OrderDto[]>(OrderHubMethods.OrderListUpdated,
                (type, orders) => observer.OnNext(Tuple.Create(type, orders)));
        }

        public ValueTask DisposeAsync() => _connection.DisposeAsync();
    }
}
