﻿using static CaptainHook.Common.Constants;

namespace CaptainHook.Common
{
    public struct ServiceNaming
    {
        public const string EventReaderServiceShortName = "EventReader";

        public static string EventReaderServiceFullUri(string eventName, string subscriberName) => $"{EventReaderServicePrefix}.{eventName}-{subscriberName}";

        public const string EventReaderServiceType = "CaptainHook.EventReaderServiceType";

        public const string DirectorServiceShortName = "Director";

        public const string DirectorServiceType = "CaptainHook.DirectorServiceType";

        public const string EventHandlerServiceShortName = "EventHandler";

        public static readonly string EventHandlerServiceFullName = $"fabric:/{CaptainHookApplication.ApplicationName}/{EventHandlerServiceShortName}";

        public static readonly string EventReaderServicePrefix = $"fabric:/{CaptainHookApplication.ApplicationName}/{EventReaderServiceShortName}";

        public static readonly string DirectorServiceFullName = $"fabric:/{CaptainHookApplication.ApplicationName}/CaptainHook.DirectorService";

        public const string EventHandlerActorServiceType = "EventHandlerActorServiceType";

        public const string ApiServiceServiceType = "CaptainHook.ApiType";
    }
}
