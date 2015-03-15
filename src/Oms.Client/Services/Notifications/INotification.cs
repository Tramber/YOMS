using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
