using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oms.Client.Services.Notifications
{
    public class Notification : INotification, IEquatable<Notification>
    {
        public Notification(string title, string description, NotificationType notificationType)
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            Id = BitConverter.ToInt64(buffer, 0);
            PublishDate = DateTime.Now;
            NotificationType = notificationType;
            Title = title;
            Description = description;
        }

        public long Id { get; private set; }

        public string Title { get; private set; }

        public string Description { get; private set; }

        public NotificationType NotificationType { get; private set; }

        public DateTime PublishDate { get; private set; }

        public bool Equals(Notification other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Notification) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

}
