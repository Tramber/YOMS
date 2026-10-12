using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Oms.Transport.Contracts;
using Oms.Transport.Contracts.Dto;

namespace Oms.Client.Services
{
    /// <summary>SignalR client of the server hub. Observe it to receive the order changes pushed by the server.</summary>
    public class OrderServiceProxy : IObservable<Tuple<ItemOperationType, OrderDto[]>>, IAsyncDisposable
    {
        private readonly HubConnection _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:9001" + OrderHubMethods.Path)
            .WithAutomaticReconnect()
            .Build();

        /// <summary>Connects to the hub and subscribes to order events. Throws when the server cannot be reached.</summary>
        public async Task StartAsync()
        {
            await _connection.StartAsync();
            await _connection.InvokeAsync(OrderHubMethods.Subscribe);
        }

        /// <summary>Calls the observer for each batch of orders pushed by the server. Dispose the result to stop.</summary>
        public IDisposable Subscribe(IObserver<Tuple<ItemOperationType, OrderDto[]>> observer)
        {
            return _connection.On<ItemOperationType, OrderDto[]>(OrderHubMethods.OrderListUpdated,
                (type, orders) => observer.OnNext(Tuple.Create(type, orders)));
        }

        public ValueTask DisposeAsync() => _connection.DisposeAsync();
    }
}
