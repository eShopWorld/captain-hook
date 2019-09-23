using System.Fabric;

namespace CaptainHook.Common.Telemetry.Service
{
    public class FailureStateUpdateException : ServiceException
    {
        public FailureStateUpdateException(string message, StatefulServiceContext context) : base(message, context)
        {

        }

        public FailureStateUpdateException(long transactionId, int handleDataHandlerId, string eventType, string message, StatefulServiceContext context) : base(message, context)
        {
            TransactionId = transactionId;
            HandlerId = handleDataHandlerId;
            EventType = eventType;
        }

        public long TransactionId { get; set; }

        public int HandlerId { get; set; }

        public string EventType { get; set; }
    }
}