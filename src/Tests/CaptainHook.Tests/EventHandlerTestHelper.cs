using System.Collections.Generic;
using CaptainHook.Common;

namespace CaptainHook.Tests
{
    public class EventHandlerTestHelper
    {
        public static (MessageData data, Dictionary<string, object> metaData) CreateMessageDataPayload()
        {
            var dictionary = new Dictionary<string, object>
            {
                {"OrderCode", "BB39357A-90E1-4B6A-9C94-14BD1A62465E"},
                {"BrandType", "Bob"},
                {"TransportModel", new { Name = "Hello World" }}
            };

            var messageData = new MessageData
            {
                Payload = dictionary.ToJson(),
                Type = "TestType",
            };

            return (messageData, dictionary);
        }
    }
}
