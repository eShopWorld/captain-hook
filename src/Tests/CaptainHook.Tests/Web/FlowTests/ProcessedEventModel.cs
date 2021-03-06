﻿using System;
using Newtonsoft.Json;

namespace CaptainHook.Tests.Web.FlowTests
{
    public class ProcessedEventModel
    {
        [JsonProperty( "eventType")]
        public string EventType { get; set; }

        [JsonProperty("payload")]
        public string Payload { get; set; }

        [JsonProperty("verb")]
        public string Verb { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        internal bool IsCallback =>
            !string.IsNullOrWhiteSpace(Url) && Url.Contains(PeterPanConsts.IntakeCallbackRouteToken, StringComparison.OrdinalIgnoreCase);

        public Uri Uri => new Uri(Url);

        [JsonProperty("Authorization")]
        public string Authorization { get; set; }

        [JsonProperty("payloadId")]
        public string PayloadId { get; set; }
    }
}
