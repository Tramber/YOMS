namespace Oms.Client.Services.Notifications
{
    /// <summary>Shows a message to the user: it is sent on the messenger and displayed by the shell.</summary>
    public interface INotificationService
    {
        void Process(string title, string description, NotificationType notificationType);
    }
}