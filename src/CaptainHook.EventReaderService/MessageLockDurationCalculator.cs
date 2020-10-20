using System;
using System.Linq;

namespace CaptainHook.EventReaderService
{
    public class MessageLockDurationCalculator : IMessageLockDurationCalculator
    {
        private static readonly TimeSpan TimeAllowance = TimeSpan.FromSeconds(5);

        // this is a Service Bus limit
        private const int MaxAllowedMessageLockDurationInSeconds = 300;

        public int CalculateAsSeconds(TimeSpan httpTimeout, TimeSpan[] retrySleepDurations)
        {
            if (retrySleepDurations == null)
            {
                throw new ArgumentNullException(nameof(retrySleepDurations));
            }

            var result =
                httpTimeout.TotalSeconds * (retrySleepDurations.Length + 1) +
                retrySleepDurations.Sum(x => x.TotalSeconds) +
                TimeAllowance.TotalSeconds;

            return result > MaxAllowedMessageLockDurationInSeconds ? 
                MaxAllowedMessageLockDurationInSeconds : 
                (int)result;
        }
    }
}
