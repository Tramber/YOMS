using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Oms.Client.Services.Notifications;

namespace Oms.Client.ViewModels
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
