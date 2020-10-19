using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CaptainHook.EventReaderService
{
    public class MessageLockDurationCalculator : IMessageLockDurationCalculator
    {
        private static readonly TimeSpan TimeAllowance = TimeSpan.FromSeconds(5);

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

            return result > 300 ? 300 : (int)result;
        }
    }
}
