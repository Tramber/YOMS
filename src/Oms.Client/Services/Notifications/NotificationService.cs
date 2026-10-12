using CommunityToolkit.Mvvm.Messaging;

namespace Oms.Client.Services.Notifications
{
    /// <summary>Sends notifications on the <see cref="IMessenger"/>.</summary>
    public class NotificationService : INotificationService
    {
        private readonly IMessenger _messenger;

        public NotificationService(IMessenger messenger)
        {
            _messenger = messenger;
        }

        public void Process(string title, string description, NotificationType notificationType = NotificationType.Info)
        {
            _messenger.Send<INotification>(new Notification(title, description, notificationType));
        }
    }
}
