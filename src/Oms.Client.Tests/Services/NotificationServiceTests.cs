using System.Collections.Generic;
using CommunityToolkit.Mvvm.Messaging;
using NUnit.Framework;
using Oms.Client.Services.Notifications;

namespace Oms.Client.Tests.Services
{
    [TestFixture]
    public class NotificationServiceTests
    {
        private WeakReferenceMessenger _messenger;
        private List<INotification> _received;
        private NotificationService _service;
        private object _recipient;

        [SetUp]
        public void SetUp()
        {
            _messenger = new WeakReferenceMessenger();
            _received = [];
            _recipient = new object();
            _messenger.Register<object, INotification>(_recipient, (_, message) => _received.Add(message));
            _service = new NotificationService(_messenger);
        }

        [Test]
        public void Should_send_the_notification_to_the_subscribers()
        {
            _service.Process("title", "description", NotificationType.Warn);

            Assert.That(_received, Has.Count.EqualTo(1));
            Assert.That(_received[0].Title, Is.EqualTo("title"));
            Assert.That(_received[0].Description, Is.EqualTo("description"));
            Assert.That(_received[0].NotificationType, Is.EqualTo(NotificationType.Warn));
        }

        [Test]
        public void Should_default_to_an_info_notification()
        {
            _service.Process("title", "description");

            Assert.That(_received[0].NotificationType, Is.EqualTo(NotificationType.Info));
        }

        [Test]
        public void Extension_methods_set_the_notification_type()
        {
            _service.ProcessInfo("a", "a");
            _service.ProcessWarn("b", "b");
            _service.ProcessError("c", "c");

            Assert.That(_received.ConvertAll(n => n.NotificationType),
                Is.EqualTo([NotificationType.Info, NotificationType.Warn, NotificationType.Error]));
        }

        [Test]
        public void Each_notification_has_its_own_identity()
        {
            _service.Process("a", "a");
            _service.Process("a", "a");

            Assert.That(_received[0], Is.Not.EqualTo(_received[1]));
            Assert.That(_received[0].Id, Is.Not.EqualTo(_received[1].Id));
        }
    }
}
