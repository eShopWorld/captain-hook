using System;

namespace CaptainHook.EventReaderService
{
    public interface IMessageLockDurationCalculator
    {
        /// <summary>
        /// Calculates the message lock duration in seconds given the http timeout and retry durations
        /// </summary>
        /// <param name="httpTimeout"></param>
        /// <param name="retrySleepDurations"></param>
        /// <returns></returns>
        int CalculateAsSeconds(TimeSpan httpTimeout, TimeSpan[] retrySleepDurations);
    }
}