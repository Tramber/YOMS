using System.ComponentModel.Composition;
using Caliburn.Micro;
using Oms.Client.Services.Notifications;

namespace Oms.Client.Modules.Notification
{
    [Export(typeof(INotificationViewModel))]
    public class NotificationViewModel : Screen, INotificationViewModel
    {
        public NotificationViewModel(INotification notification)
        {
            Notification = notification;
        }

        public INotification Notification { get; private set; }

    }
}
