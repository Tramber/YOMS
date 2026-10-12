using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Oms.Client.Modules.OrderBlotter;
using Oms.Client.Modules.OrderEditor;
using Oms.Client.Modules.Shell;
using Oms.Client.Services;
using Oms.Client.Services.Notifications;
using Oms.Transport.Contracts.Dto;

namespace Oms.Client.Tests.Modules
{
    [TestFixture]
    public class ShellViewModelTests
    {
        private WeakReferenceMessenger _messenger;
        private IWindowService _windowService;
        private IOrderServiceProxy _proxy;
        private Session _session;
        private ShellViewModel _shell;

        [SetUp]
        public void SetUp()
        {
            _messenger = new WeakReferenceMessenger();
            _windowService = Substitute.For<IWindowService>();
            _proxy = Substitute.For<IOrderServiceProxy>();
            _proxy.GetOrdersAsync().Returns(Task.FromResult(new List<OrderDto> { new OrderDto { Id = 1 } }));
            _proxy.GetSecuritiesAsync().Returns(Task.FromResult(new List<SecurityDto> { new SecurityDto { Id = 1, Name = "ACME" } }));
            _proxy.GetFundsAsync().Returns(Task.FromResult(new List<FundDto> { new FundDto { Id = 1, Name = "Alpha" } }));
            _session = new Session { Login = "alice" };
            _shell = new ShellViewModel(_windowService, new NotificationService(_messenger), _messenger, _proxy, _session);
        }

        [Test]
        public void Should_open_with_the_order_blotter_as_the_active_document()
        {
            Assert.That(_shell.Documents, Has.Count.EqualTo(1));
            Assert.That(_shell.Documents.Single(), Is.InstanceOf<OrderBlotterViewModel>());
            Assert.That(_shell.ActiveDocument, Is.SameAs(_shell.Blotter));
            Assert.That(_shell.ActiveDocument.DisplayName, Is.EqualTo("Order Blotter"));
        }

        [Test]
        public void Should_start_disconnected_and_share_the_session()
        {
            Assert.That(_shell.IsConnected, Is.False);
            Assert.That(_shell.ConnectionStatus, Is.EqualTo("Not connected"));
            Assert.That(_shell.Session, Is.SameAs(_session));
        }

        [Test]
        public async Task Should_connect_and_load_the_orders()
        {
            await _shell.ConnectCommand.ExecuteAsync(null);

            await _proxy.Received(1).ConnectAsync();
            await _proxy.Received(1).GetOrdersAsync();
            Assert.That(_shell.IsConnected, Is.True);
            Assert.That(_shell.ConnectionStatus, Is.EqualTo("Connected as alice"));
            Assert.That(_shell.Blotter.OrdersView.Cast<object>().Count(), Is.EqualTo(1));
            Assert.That(_shell.Notifications, Is.Empty);
        }

        [Test]
        public async Task Should_stay_disconnected_and_tell_the_user_when_the_server_cannot_be_reached()
        {
            _proxy.ConnectAsync().Throws(new InvalidOperationException("refused"));

            await _shell.ConnectCommand.ExecuteAsync(null);

            Assert.That(_shell.IsConnected, Is.False);
            Assert.That(_shell.ConnectionStatus, Is.EqualTo("Not connected"));
            Assert.That(_shell.Notifications, Has.Count.EqualTo(1));
            Assert.That(_shell.Notifications[0].Notification.Title, Is.EqualTo("Connection failed"));
            Assert.That(_shell.Notifications[0].Notification.Description, Is.EqualTo("refused"));
            Assert.That(_shell.Notifications[0].Notification.NotificationType, Is.EqualTo(NotificationType.Error));
            await _proxy.DidNotReceive().GetOrdersAsync();
        }

        [Test]
        public async Task Should_stay_disconnected_when_the_orders_cannot_be_loaded()
        {
            _proxy.GetOrdersAsync().Throws(new InvalidOperationException("boom"));

            await _shell.ConnectCommand.ExecuteAsync(null);

            Assert.That(_shell.IsConnected, Is.False);
            Assert.That(_shell.Notifications, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task Should_connect_only_once()
        {
            await _shell.ConnectCommand.ExecuteAsync(null);
            await _shell.ConnectCommand.ExecuteAsync(null);

            await _proxy.Received(1).ConnectAsync();
        }

        [Test]
        public async Task Should_try_again_after_a_failed_connection()
        {
            _proxy.ConnectAsync().Returns(Task.FromException(new InvalidOperationException("refused")), Task.CompletedTask);

            await _shell.ConnectCommand.ExecuteAsync(null);
            await _shell.ConnectCommand.ExecuteAsync(null);

            Assert.That(_shell.IsConnected, Is.True);
        }

        [Test]
        public async Task Should_only_allow_creating_an_order_once_connected()
        {
            Assert.That(_shell.CreateOrderCommand.CanExecute(null), Is.False);

            await _shell.ConnectCommand.ExecuteAsync(null);

            Assert.That(_shell.CreateOrderCommand.CanExecute(null), Is.True);
        }

        [Test]
        public async Task Should_open_the_order_editor_loaded_with_the_securities_and_funds()
        {
            OrderEditorViewModel opened = null;
            _windowService.ShowDialogAsync(Arg.Do<object>(vm => opened = vm as OrderEditorViewModel)).Returns(Task.CompletedTask);
            await _shell.ConnectCommand.ExecuteAsync(null);

            await _shell.CreateOrderCommand.ExecuteAsync(null);

            Assert.That(opened, Is.Not.Null);
            Assert.That(opened.Securities.Single().Name, Is.EqualTo("ACME"));
            Assert.That(opened.SelectedFund.Name, Is.EqualTo("Alpha"));
        }

        [Test]
        public async Task Should_not_open_the_order_editor_when_its_lists_cannot_be_loaded()
        {
            await _shell.ConnectCommand.ExecuteAsync(null);
            _proxy.GetSecuritiesAsync().Throws(new InvalidOperationException("no securities"));

            await _shell.CreateOrderCommand.ExecuteAsync(null);

            await _windowService.DidNotReceive().ShowDialogAsync(Arg.Any<object>());
            Assert.That(_shell.Notifications.Single().Notification.Title, Is.EqualTo("Order editor"));
            Assert.That(_shell.Notifications.Single().Notification.Description, Is.EqualTo("no securities"));
        }

        [Test]
        public void Should_show_a_notification_sent_on_the_messenger()
        {
            new NotificationService(_messenger).ProcessWarn("careful", "details");

            Assert.That(_shell.Notifications, Has.Count.EqualTo(1));
            Assert.That(_shell.Notifications[0].Notification.Title, Is.EqualTo("careful"));
            Assert.That(_shell.Notifications[0].Notification.NotificationType, Is.EqualTo(NotificationType.Warn));
        }
    }
}
