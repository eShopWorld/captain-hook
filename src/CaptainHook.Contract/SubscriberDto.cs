﻿namespace CaptainHook.Contract
{
    public class SubscriberDto
    {
        public WebhooksDto Webhooks { get; set; }

        public WebhooksDto Callbacks { get; set; }
        
        public WebhooksDto DlqHooks { get; set; }
    }
}
