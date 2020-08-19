using System.Collections.Generic;
using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Application.Validators;
using CaptainHook.Contract;
using CaptainHook.TestsInfrastructure.Builders;
using CaptainHook.TestsInfrastructure.TestsData;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace CaptainHook.Application.Tests.RequestValidators
{
    public class UpsertWebhookRequestValidatorTests
    {
        private readonly UpsertWebhookRequestValidator _validator = new UpsertWebhookRequestValidator();

        [Fact, IsUnit]
        public void When_RequestIsValid_Then_NoFailuresReturned()
        {
            var request = new UpsertWebhookRequest("event", "subscriber", new EndpointDtoBuilder().Create());

            var result = _validator.TestValidate(request);

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

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_EventIsEmpty_Then_ValidationFails(string invalidString)
        {
            var request = new UpsertWebhookRequest(invalidString, "subscriber", new EndpointDtoBuilder().Create());

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.EventName);
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_SubscriberIsEmpty_Then_ValidationFails(string invalidString)
        {
            var request = new UpsertWebhookRequest("event", invalidString, new EndpointDtoBuilder().Create());

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.SubscriberName);
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_UriIsEmpty_Then_ValidationFails(string uri)
        {
            var dto = new EndpointDtoBuilder().With(x => x.Uri, uri).Create();
            var request = new UpsertWebhookRequest("event", "subscriber", dto);

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Endpoint.Uri);
        }

        [Theory, IsUnit]
        [ClassData(typeof(InvalidUris))]
        public void When_UriIsNotValid_Then_ValidationFails(string uri)
        {
            var dto = new EndpointDtoBuilder().With(x => x.Uri, uri).Create();
            var request = new UpsertWebhookRequest("event", "subscriber", dto);

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Endpoint.Uri);
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_HttpVerbIsEmpty_Then_ValidationFails(string httpVerb)
        {
            var dto = new EndpointDtoBuilder().With(x => x.HttpVerb, httpVerb).Create();
            var request = new UpsertWebhookRequest("event", "subscriber", dto);

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Endpoint.HttpVerb);
        }

        [Theory, IsUnit]
        [InlineData("duck")]
        public void When_HttpVerbIsNotValidHTTPVerb_Then_ValidationFails(string httpVerb)
        {
            var dto = new EndpointDtoBuilder().With(x => x.HttpVerb, httpVerb).Create();
            var request = new UpsertWebhookRequest("event", "subscriber", dto);

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Endpoint.HttpVerb);
        }

        [Fact, IsUnit]
        public void When_UriTransformIsValid_Then_NoFailuresReturned()
        {
            var dto = new EndpointDtoBuilder()
                .With(x => x.Uri, "https://blah-{selector}.eshopworld.com/webhook/")
                .Create();
            var request = new UpsertWebhookRequest("event", "subscriber", dto);

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
