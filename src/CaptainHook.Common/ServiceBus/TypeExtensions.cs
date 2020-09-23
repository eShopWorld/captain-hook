using System;

namespace CaptainHook.Common.ServiceBus
{


    /// <summary>
    /// Contains extensions methods that are <see cref="IServiceBusNamespace"/> related but extend
    /// the system <see cref="Type"/> instead.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Gets a queue/topic name off a system <see cref="Type"/> by using the <see cref="Type"/>.FullName.
        /// If you're in DEBUG with the debugger attached, then the full name is appended by a '-' followed by
        /// an Environment.Username, giving you a named queue during debug cycles to avoid queue name clashes.
        /// </summary>
        /// <param name="type">The message <see cref="Type"/> that this topic is for.</param>
        /// <returns>The final name of the topic.</returns>
        public static string GetEntityName(string type)
        {
            var name = type;
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                name += $"-{Environment.UserName.Replace("$", "")}";
            }
#endif

            return name?.ToLower();
        }
    }
}