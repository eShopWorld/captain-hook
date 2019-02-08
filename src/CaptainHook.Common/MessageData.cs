namespace CaptainHook.Common
{
    using System;

    public class MessageData
    {
        public Guid Handle { get; set; }

        public string Payload { get; set; }

        public string Type { get; set; }

        [Obsolete]
        public Guid OrderCode { get; set; }

        /// <summary>
        /// todo remove when webhooks do not need a guid in the uri path
        /// </summary>
        [Obsolete]
        public string CallbackPayload { get; set; }
    }
}
