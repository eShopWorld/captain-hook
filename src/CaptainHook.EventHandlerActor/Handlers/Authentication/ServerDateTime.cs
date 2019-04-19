using System;

namespace CaptainHook.EventHandlerActor.Handlers.Authentication
{
    internal static class ServerDateTime
    {
        /// <summary>
        ///     The current UTC date time
        /// </summary>
        public static DateTime UtcNow => UtcNowFunc();

        /// <summary>
        ///     The function used to resolve the date time
        /// </summary>
        public static Func<DateTime> UtcNowFunc { get; set; } = () => DateTime.UtcNow;
    }
}