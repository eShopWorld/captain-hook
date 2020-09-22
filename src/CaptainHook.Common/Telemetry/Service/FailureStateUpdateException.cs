﻿using System;
using System.Fabric;
using System.Runtime.Serialization;

namespace CaptainHook.Common.Telemetry.Service
{
    [Serializable]
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

        private FailureStateUpdateException(SerializationInfo info, StreamingContext context)
        : base(info, context)
        {
            // ...
        }

        public long TransactionId { get; set; }

        public int HandlerId { get; set; }

        public string EventType { get; set; }
    }
}