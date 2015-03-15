namespace Oms.Client.Services.Notifications
{
    public static class NotificationServiceExtensions
    {
        public static void ProcessInfo(this INotificationService notificationService, string title, string description)
        {
            notificationService.Process(title, description, NotificationType.Info);
        }

        public static void ProcessWarn(this INotificationService notificationService, string title, string description)
        {
            notificationService.Process(title, description, NotificationType.Warn);
        }

        public static void ProcessError(this INotificationService notificationService, string title, string description)
        {
            notificationService.Process(title, description, NotificationType.Error);
        }
    }
}