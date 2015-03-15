namespace Oms.Client.Services.Notifications
{
    public interface INotificationService
    {
        void Process(string title, string description, NotificationType notificationType);
    }
}