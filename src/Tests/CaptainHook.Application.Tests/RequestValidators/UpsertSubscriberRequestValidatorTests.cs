using System.Collections.Generic;
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
        private readonly UpsertWebhookRequestValidator _validator = new UpsertWebhookRequestValidator();

        private readonly AuthenticationDto _validOidcAuthentication = new SimpleBuilder<AuthenticationDto>()
            .With(x => x.Type, "OIDC")
            .With(x => x.ClientId, "clientId")
            .With(x => x.Uri, "https://security-api.com/token")
            .With(x => x.Scopes, new List<string>(new[] { "test.scope.api" }))
            .With(x => x.ClientSecret, new ClientSecretDto { Name = "secret-name", Vault = "secret-vault" })
            .Create();

        [Fact, IsUnit]
        public void When_request_is_valid_then_no_failures_should_be_returned()
        {
            var dto = new SimpleBuilder<EndpointDto>()
                .With(x => x.Uri, "https://blah.blah.eshopworld.com/webhook/")
                .With(x => x.HttpVerb, "POST")
                .With(x => x.Authentication, _validOidcAuthentication)
                .Create();
            var request = new UpsertWebhookRequest("event", "subscriber", dto);

            var result = _validator.Validate(request);

            result.IsValid.Should().BeTrue();
        }

        [Theory, IsUnit]
        [InlineData("POST")]
        [InlineData("GET")]
        [InlineData("PUT")]
        public void When_HttpVerb_is_valid_HTTP_verb_then_then_no_failures_should_be_returned(string httpVerb)
        {
            var dto = new SimpleBuilder<EndpointDto>()
                .With(x => x.Uri, "https://blah.blah.eshopworld.com/webhook/")
                .With(x => x.HttpVerb, httpVerb)
                .With(x => x.Authentication, _validOidcAuthentication)
                .Create();
            var request = new UpsertWebhookRequest("event", "subscriber", dto);

            var result = _validator.Validate(request);

            result.IsValid.Should().BeTrue();
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_Uri_is_empty_then_then_validation_should_fail(string uri)
        {
            var dto = new SimpleBuilder<EndpointDto>()
                .With(x => x.Uri, uri)
                .With(x => x.HttpVerb, "POST")
                .With(x => x.Authentication, _validOidcAuthentication)
                .Create();
            var request = new UpsertWebhookRequest("event", "subscriber", dto);

            var result = _validator.Validate(request);

            result.AssertSingleFailure(nameof(EndpointDto.Uri));
        }

        [Theory, IsUnit]
        [ClassData(typeof(InvalidUris))]
        public void When_Uri_is_not_a_valid_uri_then_validation_should_fail(string uri)
        {
            var dto = new SimpleBuilder<EndpointDto>()
                .With(x => x.Uri, uri)
                .With(x => x.HttpVerb, "POST")
                .With(x => x.Authentication, _validOidcAuthentication)
                .Create();
            var request = new UpsertWebhookRequest("event", "subscriber", dto);

            var result = _validator.Validate(request);

            result.AssertSingleFailure(nameof(EndpointDto.Uri));
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_HttpVerb_is_empty_then_then_validation_should_fail(string httpVerb)
        {
            var dto = new SimpleBuilder<EndpointDto>()
                .With(x => x.Uri, "https://blah.blah.eshopworld.com/webhook/")
                .With(x => x.HttpVerb, httpVerb)
                .With(x => x.Authentication, _validOidcAuthentication)
                .Create();
            var request = new UpsertWebhookRequest("event", "subscriber", dto);

            var result = _validator.Validate(request);

            result.AssertSingleFailure(nameof(EndpointDto.HttpVerb));
        }

        [Theory, IsUnit]
        [InlineData("duck")]
        public void When_HttpVerb_is_not_a_valid_HTTP_verb_then_then_validation_should_fail(string httpVerb)
        {
            var dto = new SimpleBuilder<EndpointDto>()
                .With(x => x.Uri, "https://blah.blah.eshopworld.com/webhook/")
                .With(x => x.HttpVerb, httpVerb)
                .With(x => x.Authentication, _validOidcAuthentication)
                .Create();
            var request = new UpsertWebhookRequest("event", "subscriber", dto);

            var result = _validator.Validate(request);

            result.AssertSingleFailure(nameof(EndpointDto.HttpVerb));
        }
    }
}
