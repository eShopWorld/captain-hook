using System;

namespace CaptainHook.EventReaderService
{
    public interface IMessageLockDurationCalculator
    {
        /// <summary>
        /// Calculates the message lock duration in seconds given the http timeout and retry durations
        /// </summary>
        /// <param name="httpTimeout">HTTP timeout</param>
        /// <param name="retrySleepDurations">Retry intervals duration</param>
        /// <returns>Returns calculated lock duration in seconds</returns>
        int CalculateAsSeconds(TimeSpan httpTimeout, TimeSpan[] retrySleepDurations);
    }
}