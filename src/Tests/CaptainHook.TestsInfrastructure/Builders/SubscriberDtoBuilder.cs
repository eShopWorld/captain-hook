using System.Collections.Generic;
using CaptainHook.Contract;

namespace CaptainHook.TestsInfrastructure.Builders
{
    public class SubscriberDtoBuilder: SimpleBuilder<SubscriberDto>
    {
        public SubscriberDtoBuilder()
        {
            With(
                s => s.Webhooks,
                new WebhooksDto
                {
                    SelectionRule = "$.TestSelector",
                    Endpoints = new List<EndpointDto>
                    {
                        new EndpointDtoBuilder().Create()
                    }
                });
        }
    }
}