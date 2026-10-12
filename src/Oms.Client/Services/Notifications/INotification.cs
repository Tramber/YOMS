using System;

namespace Oms.Client.Services.Notifications
{
    public interface INotification
    {
        long Id { get; }
        string Title { get; }
        string Description { get; }
        NotificationType NotificationType { get; }
        DateTime PublishDate { get; }
    }
}
