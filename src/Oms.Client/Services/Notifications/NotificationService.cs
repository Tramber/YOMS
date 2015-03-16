using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Caliburn.Micro;


namespace Oms.Client.Services.Notifications
{
    [Export(typeof(INotificationService))]
    public class NotificationService : INotificationService
    {
        private readonly IEventAggregator _eventAggregator;

        public void Process(string title, string description, NotificationType notificationType = NotificationType.Info)
        {
            var notification = new Notification(title, description, notificationType);
            _eventAggregator.Publish(notification, action => action());
        }

        [ImportingConstructor]
        public NotificationService(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }
    }
}