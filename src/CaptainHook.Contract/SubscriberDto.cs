namespace CaptainHook.Contract
{
    public class SubscriberDto
    {
        public string SubscriberName { get; set; }
        public WebhooksDto Webhooks { get; set; }
    }

    /// <summary>
    /// Defines different type of subscribers
    /// </summary>
    public enum SubscriberType
    {
        /// <summary>
        /// Webhook delivery
        /// </summary>
        Webhooks = 0,

        /// <summary>
        /// Callback delivery
        /// </summary>
        Callbacks,

        /// <summary>
        /// Delivery when message is in DLQ
        /// </summary>
        DLQs
    }
}
