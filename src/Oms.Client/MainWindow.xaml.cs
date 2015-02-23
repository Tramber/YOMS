using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.Remoting.Proxies;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Oms.Client.Proxy;
using Oms.Transport.Contracts.Dto;
using IOrderServiceContractCallback = Oms.Client.Proxy.IOrderServiceContractCallback;

namespace Oms.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        private readonly OrderServiceContractClient _orderServiceClient;
        private readonly ObservableCollection<OrderDto> _orders = new ObservableCollection<OrderDto>();
        private OrderServiceContractCallbackObserver _callback;

        public MainWindow()
        {
            InitializeComponent();
            _callback = new OrderServiceContractCallbackObserver(Orders);
            _orderServiceClient = new OrderServiceContractClient(new InstanceContext(callback));
        }

        public ObservableCollection<OrderDto> Orders
        {
            get { return _orders; }
        }

        public void Dispose()
        {
            if (_orderServiceClient != null)
            {
                ((IDisposable)_orderServiceClient).Dispose();
            }
        }



        private void Subscribe_Click(object sender, RoutedEventArgs e)
        {
            _orderServiceClient.SubscribeOrders();
            Observable.Create(_callback.Orders_OnNext())
        }

        private void Unsubcribe_Click(object sender, RoutedEventArgs e)
        {
            _orderServiceClient.UnsubcribeOrders();
        }
    }

    public class OrderServiceContractCallbackObserver : IOrderServiceContractCallback
    {
        private readonly ICollection<OrderDto> _orders;

        public OrderServiceContractCallbackObserver(ICollection<OrderDto> orders)
        {
            Contract.Requires(orders != null);
        }

        public void Orders_OnNext(OrderDto orderDto)
        {
            
        }

        public Action<OrderDto> OrdersOnNext;


    }




}
