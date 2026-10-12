using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using NSubstitute;
using NUnit.Framework;
using Oms.Client.Modules.OrderBlotter;
using Oms.Client.Modules.OrderEditor;
using Oms.Client.Modules.Shell;
using Oms.Client.Services;
using Oms.Client.Services.Notifications;

namespace Oms.Client.Tests.Modules
{
    [TestFixture]
    public class ShellViewModelTests
    {
        private WeakReferenceMessenger _messenger;
        private IWindowService _windowService;
        private ShellViewModel _shell;

        [SetUp]
        public void SetUp()
        {
            _messenger = new WeakReferenceMessenger();
            _windowService = Substitute.For<IWindowService>();
            _shell = new ShellViewModel(_windowService, new NotificationService(_messenger), _messenger);
        }

        [Test]
        public void Should_open_with_the_order_blotter_as_the_active_document()
        {
            Assert.That(_shell.Documents, Has.Count.EqualTo(1));
            Assert.That(_shell.Documents.Single(), Is.InstanceOf<OrderBlotterViewModel>());
            Assert.That(_shell.ActiveDocument, Is.SameAs(_shell.Documents.Single()));
            Assert.That(_shell.ActiveDocument.DisplayName, Is.EqualTo("Order Blotter"));
        }

        [Test]
        public void Should_show_a_notification_sent_on_the_messenger()
        {
            new NotificationService(_messenger).ProcessError("failed", "details");

            Assert.That(_shell.Notifications, Has.Count.EqualTo(1));
            Assert.That(_shell.Notifications[0].Notification.Title, Is.EqualTo("failed"));
        }

        [Test]
        public void Create_notification_command_adds_a_notification()
        {
            _shell.CreateNotificationCommand.Execute(null);

            Assert.That(_shell.Notifications, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task Create_order_command_opens_the_order_editor_dialog()
        {
            await _shell.CreateOrderCommand.ExecuteAsync(null);

            await _windowService.Received(1).ShowDialogAsync(Arg.Any<OrderEditorViewModel>());
        }
    }
}
