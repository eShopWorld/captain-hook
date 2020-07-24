using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Application.RequestValidators;
using CaptainHook.Contract;
using CaptainHook.TestsInfrastructure;
using CaptainHook.TestsInfrastructure.Builders;
using CaptainHook.TestsInfrastructure.TestsData;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Xunit;

namespace CaptainHook.Application.Tests.RequestValidators
{
    public class UpsertSubscriberRequestValidatorTests
    {
        private readonly UpsertWebhookRequestValidator _validator = new UpsertWebhookRequestValidator();

        [Fact, IsUnit]
        public void When_request_is_valid_then_no_failures_should_be_returned()
        {
            var request = new UpsertWebhookRequest("event", "subscriber", new EndpointDtoBuilder().Create());

            var result = _validator.Validate(request);

            result.IsValid.Should().BeTrue();
        }

        [Theory, IsUnit]
        [InlineData("POST")]
        [InlineData("GET")]
        [InlineData("PUT")]
        public void When_HttpVerb_is_valid_HTTP_verb_then_then_no_failures_should_be_returned(string httpVerb)
        {
            var dto = new EndpointDtoBuilder().With(x => x.HttpVerb, httpVerb).Create();
            var request = new UpsertWebhookRequest("event", "subscriber", dto);

            var result = _validator.Validate(request);

            result.IsValid.Should().BeTrue();
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_Event_is_empty_then_then_validation_should_fail(string invalidString)
        {
            var request = new UpsertWebhookRequest(invalidString, "subscriber", new EndpointDtoBuilder().Create());

            var result = _validator.Validate(request);

            result.AssertSingleFailure(nameof(EndpointDto.Uri));
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_Subscriber_is_empty_then_then_validation_should_fail(string invalidString)
        {
            var request = new UpsertWebhookRequest("event", invalidString, new EndpointDtoBuilder().Create());

            var result = _validator.Validate(request);

            result.AssertSingleFailure(nameof(EndpointDto.Uri));
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_Uri_is_empty_then_then_validation_should_fail(string uri)
        {
            var dto = new EndpointDtoBuilder().With(x => x.Uri, uri).Create();
            var request = new UpsertWebhookRequest("event", "subscriber", dto);

            var result = _validator.Validate(request);

            result.AssertSingleFailure(nameof(EndpointDto.Uri));
        }

        [Theory, IsUnit]
        [ClassData(typeof(InvalidUris))]
        public void When_Uri_is_not_a_valid_uri_then_validation_should_fail(string uri)
        {
            var dto = new EndpointDtoBuilder().With(x => x.Uri, uri).Create();
            var request = new UpsertWebhookRequest("event", "subscriber", dto);

            var result = _validator.Validate(request);

            result.AssertSingleFailure(nameof(EndpointDto.Uri));
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_HttpVerb_is_empty_then_then_validation_should_fail(string httpVerb)
        {
            var dto = new EndpointDtoBuilder().With(x => x.HttpVerb, httpVerb).Create();
            var request = new UpsertWebhookRequest("event", "subscriber", dto);

            var result = _validator.Validate(request);

            result.AssertSingleFailure(nameof(EndpointDto.HttpVerb));
        }

        [Theory, IsUnit]
        [InlineData("duck")]
        public void When_HttpVerb_is_not_a_valid_HTTP_verb_then_then_validation_should_fail(string httpVerb)
        {
            var dto = new EndpointDtoBuilder().With(x => x.HttpVerb, httpVerb).Create();
            var request = new UpsertWebhookRequest("event", "subscriber", dto);

            var result = _validator.Validate(request);

            result.AssertSingleFailure(nameof(EndpointDto.HttpVerb));
        }
    }
}
