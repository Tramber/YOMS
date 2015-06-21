using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oms.Server.Domain.Models.EventLogs
{
    public static class EventLogExtensions
    {
        public static bool IsPendingReply(this TriggerStatus triggerStatus)
        {
            return triggerStatus == TriggerStatus.Rejected || triggerStatus == TriggerStatus.Accepted;
        }

        public static bool IsValid(this TriggerStatus triggerStatus)
        {
            return triggerStatus == TriggerStatus.Accepted || triggerStatus == TriggerStatus.Done;
        }

        public static bool IsPending(this TriggerStatus triggerStatus)
        {
            return triggerStatus == TriggerStatus.Pending;
        }

        public static bool IsFailure(this TriggerStatus triggerStatus)
        {
            return triggerStatus == TriggerStatus.Failed;
        }

        public static bool IsUnvalid(this TriggerStatus triggerStatus)
        {
            return triggerStatus == TriggerStatus.Rejected || triggerStatus == TriggerStatus.Failed
                   || triggerStatus == TriggerStatus.Pending;
        }
    }
}
