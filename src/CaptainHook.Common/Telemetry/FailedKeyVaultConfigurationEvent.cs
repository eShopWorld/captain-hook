using System;
using Eshopworld.Core;

namespace CaptainHook.Common.Telemetry
{
    internal class FailedKeyVaultConfigurationEvent : ExceptionEvent
    {
        public FailedKeyVaultConfigurationEvent(Exception exception) : base(exception)
        {
        }

        public string ConfigKey { get; set; }
        public string SubscriberKey { get; set; }
    }
}
