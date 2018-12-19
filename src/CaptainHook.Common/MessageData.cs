namespace CaptainHook.Common
{
    using System;
    using Nasty;

    public class MessageData
    {
        public Guid Handle { get; set; }

        public string Payload { get; set; }

        public string Type { get; set; }

        /// <summary>
        /// todo remove when webhooks do not need a guid in the uri path
        /// </summary>
        public HttpResponseDto CallbackPayload { get; set; }
    }
}
