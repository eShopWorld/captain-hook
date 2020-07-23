using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Application.RequestValidators;
using CaptainHook.Contract;
using CaptainHook.TestsInfrastructure;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Xunit;

namespace CaptainHook.Application.Tests.RequestValidators
{
    public class UpsertSubscriberRequestValidatorTests
    {
        [Fact, IsUnit]
        public void When_request_is_valid_then_no_failures_should_be_returned()
        {
            var dto = new SimpleBuilder<EndpointDto>()
                .With(x => x.Uri, "https://blah.blah.eshopworld.com/webhook/")
                .With(x => x.HttpVerb, "POST")
                .Create();
            var request = new UpsertWebhookRequest("event", "subscriber", dto);

            var result = new UpsertWebhookRequestValidator().Validate(request);

            result.IsValid.Should().BeTrue();
        }
    }
}
