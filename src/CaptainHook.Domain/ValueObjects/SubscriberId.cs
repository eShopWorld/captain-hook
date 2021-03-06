﻿namespace CaptainHook.Domain.ValueObjects
{
    public class SubscriberId
    {
        private readonly string _id;
        public string EventName { get; }
        public string SubscriberName { get; }

        public SubscriberId(string eventName, string subscriberName)
        {
            EventName = eventName;
            SubscriberName = subscriberName;
            _id = $"{eventName}-{subscriberName}";
        }

        public override bool Equals(object obj)
        {
            var otherId = obj as SubscriberId;
            return _id == otherId?._id;
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public override string ToString()
        {
            return _id;
        }

        public static implicit operator string(SubscriberId subscriberId)
        {
            return subscriberId?.ToString();
        }
    }
}
