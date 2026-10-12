using Oms.Client.Services.Notifications;

namespace Oms.Client.Modules.Notification
{
    public class NotificationViewModel
    {
        public NotificationViewModel(INotification notification)
        {
            Notification = notification;
        }

        public INotification Notification { get; private set; }
    }
}
