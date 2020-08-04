using System.Collections.Generic;
using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Application.Validators;
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
        public void When_RequestIsValid_Then_NoFailuresReturned()
        {
            var request = new UpsertWebhookRequest("event", "subscriber", new EndpointDtoBuilder().Create());

            var result = _validator.Validate(request);

            result.IsValid.Should().BeTrue();
        }

        [Theory, IsUnit]
        [InlineData("POST")]
        [InlineData("GET")]
        [InlineData("PUT")]
        [InlineData("post")]
        [InlineData("get")]
        [InlineData("put")]
        public void When_HttpVerbIsAValidHTTPVerb_Then_NoFailuresReturned(string httpVerb)
        {
            var dto = new EndpointDtoBuilder().With(x => x.HttpVerb, httpVerb).Create();
            var request = new UpsertWebhookRequest("event", "subscriber", dto);

            var result = _validator.Validate(request);

            result.IsValid.Should().BeTrue();
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_EventIsEmpty_Then_ValidationFails(string invalidString)
        {
            var request = new UpsertWebhookRequest(invalidString, "subscriber", new EndpointDtoBuilder().Create());

            var result = _validator.Validate(request);

            result.AssertSingleFailure(nameof(UpsertWebhookRequest.EventName));
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_SubscriberIsEmpty_Then_ValidationFails(string invalidString)
        {
            var request = new UpsertWebhookRequest("event", invalidString, new EndpointDtoBuilder().Create());

            var result = _validator.Validate(request);

            result.AssertSingleFailure(nameof(UpsertWebhookRequest.SubscriberName));
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_UriIsEmpty_Then_ValidationFails(string uri)
        {
            var dto = new EndpointDtoBuilder().With(x => x.Uri, uri).Create();
            var request = new UpsertWebhookRequest("event", "subscriber", dto);

            var result = _validator.Validate(request);

            result.AssertSingleFailure(nameof(EndpointDto.Uri));
        }

        [Theory, IsUnit]
        [ClassData(typeof(InvalidUris))]
        public void When_UriIsNotValid_Then_ValidationFails(string uri)
        {
            var dto = new EndpointDtoBuilder().With(x => x.Uri, uri).Create();
            var request = new UpsertWebhookRequest("event", "subscriber", dto);

            var result = _validator.Validate(request);

            result.AssertSingleFailure(nameof(EndpointDto.Uri));
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_HttpVerbIsEmpty_Then_ValidationFails(string httpVerb)
        {
            var dto = new EndpointDtoBuilder().With(x => x.HttpVerb, httpVerb).Create();
            var request = new UpsertWebhookRequest("event", "subscriber", dto);

            var result = _validator.Validate(request);

            result.AssertSingleFailure(nameof(EndpointDto.HttpVerb));
        }

        [Theory, IsUnit]
        [InlineData("duck")]
        public void When_HttpVerbIsNotValidHTTPVerb_Then_ValidationFails(string httpVerb)
        {
            var dto = new EndpointDtoBuilder().With(x => x.HttpVerb, httpVerb).Create();
            var request = new UpsertWebhookRequest("event", "subscriber", dto);

            var result = _validator.Validate(request);

            result.AssertSingleFailure(nameof(EndpointDto.HttpVerb));
        }

        [Fact, IsUnit]
        public void When_UriTransformIsValid_Then_NoFailuresReturned()
        {
            var dto = new EndpointDtoBuilder()
                .With(x => x.Uri, "https://blah-{selector}.eshopworld.com/webhook/")
                .With(e => e.UriTransform, new UriTransformDto { Replace = new Dictionary<string, string> { ["selector"] = "$.TenantCode" } })
                .Create();
            var request = new UpsertWebhookRequest("event", "subscriber", dto);

            var result = _validator.Validate(request);

            result.IsValid.Should().BeTrue();
        }

        [Fact, IsUnit]
        public void When_UriTransformRoutesAreEmpty_Then_ValidationFails()
        {
            var dto = new EndpointDtoBuilder()
                .With(x => x.Uri, "https://blah-{selector}.eshopworld.com/webhook/")
                .With(x => x.UriTransform, new UriTransformDto { Replace = new Dictionary<string, string>() })
                .Create();
            var request = new UpsertWebhookRequest("event", "subscriber", dto);

            var result = _validator.Validate(request);

            result.AssertSingleFailure(nameof(UriTransformDto.Replace));
        }

        [Fact, IsUnit]
        public void When_UriTransformRoutesDoesNotContainSelector_Then_ValidationFails()
        {
            var dto = new EndpointDtoBuilder()
                .With(x => x.Uri, "https://blah-{selector}.eshopworld.com/webhook/")
                .With(e => e.UriTransform, new UriTransformDto { Replace = new Dictionary<string, string> { ["abc"] = "def" } })
                .Create();
            var request = new UpsertWebhookRequest("event", "subscriber", dto);

            var result = _validator.Validate(request);

            result.AssertSingleFailure(nameof(UriTransformDto.Replace));
        }

        [Fact, IsUnit]
        public void When_UriTransformRoutesDoesNotContainReplacementWhichExistInUri_Then_ValidationFails()
        {
            var dto = new EndpointDtoBuilder()
                .With(x => x.Uri, "https://blah-{selector}.eshopworld.com/webhook/{missing}")
                .With(e => e.UriTransform, new UriTransformDto { Replace = new Dictionary<string, string> { ["selector"] = "abc" } })
                .Create();
            var request = new UpsertWebhookRequest("event", "subscriber", dto);

            var result = _validator.Validate(request);

            result.AssertSingleFailure(nameof(UriTransformDto.Replace));
        }
    }
}
