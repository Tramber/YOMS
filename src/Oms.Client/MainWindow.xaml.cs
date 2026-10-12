using System;
using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.AspNetCore.SignalR.Client;
using Oms.Transport.Contracts;
using Oms.Transport.Contracts.Dto;

namespace Oms.Client
{
    public partial class MainWindow : Window, IDisposable
    {
        private readonly HubConnection _connection;

        public MainWindow()
        {
            InitializeComponent();
            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:9001" + OrderHubMethods.Path)
                .WithAutomaticReconnect()
                .Build();
            _connection.On<ItemOperationType, OrderDto[]>(OrderHubMethods.OrderListUpdated, (type, orders) =>
                Dispatcher.Invoke(() =>
                {
                    foreach (var order in orders)
                        Orders.Add(order);
                }));
        }

        public ObservableCollection<OrderDto> Orders { get; } = new ObservableCollection<OrderDto>();

        public void Dispose()
        {
            _connection.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }

        private async void Subscribe_Click(object sender, RoutedEventArgs e)
        {
            if (_connection.State == HubConnectionState.Disconnected)
                await _connection.StartAsync();
            await _connection.InvokeAsync(OrderHubMethods.Subscribe);
        }

        private async void Unsubcribe_Click(object sender, RoutedEventArgs e)
        {
            await _connection.InvokeAsync(OrderHubMethods.Unsubscribe);
        }
    }
}
