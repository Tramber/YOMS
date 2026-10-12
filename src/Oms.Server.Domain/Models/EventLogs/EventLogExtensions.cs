namespace Oms.Server.Domain.Models.EventLogs
{
    public static class EventLogExtensions
    {
        /// <summary>True for Accepted and Rejected: the answer to a pending trigger.</summary>
        public static bool IsPendingReply(this TriggerStatus triggerStatus)
        {
            return triggerStatus == TriggerStatus.Rejected || triggerStatus == TriggerStatus.Accepted;
        }

        /// <summary>True for Accepted and Done: the trigger took effect.</summary>
        public static bool IsValid(this TriggerStatus triggerStatus)
        {
            return triggerStatus == TriggerStatus.Accepted || triggerStatus == TriggerStatus.Done;
        }

        /// <summary>True while the trigger waits for an answer.</summary>
        public static bool IsPending(this TriggerStatus triggerStatus)
        {
            return triggerStatus == TriggerStatus.Pending;
        }

        /// <summary>True when the trigger failed and must be ignored when rebuilding the order.</summary>
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
