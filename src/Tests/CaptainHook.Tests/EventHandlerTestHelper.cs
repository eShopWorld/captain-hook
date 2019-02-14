using System.Collections.Generic;
using CaptainHook.Common;
using Newtonsoft.Json;

namespace CaptainHook.Tests
{
    public class EventHandlerTestHelper
    {
        public static (MessageData data, Dictionary<string, string> metaData) CreateMessageDataPayload()
        {
            var dictionary = new Dictionary<string, string>
            {
                {"OrderCode", "BB39357A-90E1-4B6A-9C94-14BD1A62465E"},
                {"BrandType", "Bob"},
                {"TransportModel", JsonConvert.SerializeObject(new {Name = "Hello World"}) }
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
