using System.Collections.Generic;
using CaptainHook.Application.Validators.Dtos;
using CaptainHook.Contract;
using CaptainHook.TestsInfrastructure;
using CaptainHook.TestsInfrastructure.Builders;
using CaptainHook.TestsInfrastructure.TestsData;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Xunit;

namespace CaptainHook.Application.Tests.RequestValidators
{
    public class AuthenticationDtoValidatorTests
    {
        private readonly AuthenticationDtoValidator _validator = new AuthenticationDtoValidator();

        [Fact, IsUnit]
        public void When_request_is_valid_then_no_failures_should_be_returned()
        {
            var dto = new AuthenticationDtoBuilder().Create();

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeTrue();
        }

        [Fact, IsUnit]
        public void When_Type_is_lowercase_then_no_failures_should_be_returned()
        {
            var dto = new AuthenticationDtoBuilder().With(x => x.Type, "oidc").Create();

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeTrue();
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_Type_is_empty_then_validation_should_fail(string invalidString)
        {
            var dto = new AuthenticationDtoBuilder().With(x => x.Type, invalidString).Create();

            var result = _validator.Validate(dto);

            result.AssertSingleFailure(nameof(AuthenticationDto.Type));
        }

        [Theory, IsUnit]
        [InlineData("unknown type")]
        public void When_Type_is_unknown_then_validation_should_fail(string invalidString)
        {
            var dto = new AuthenticationDtoBuilder().With(x => x.Type, invalidString).Create();

            var result = _validator.Validate(dto);

            result.AssertSingleFailure(nameof(AuthenticationDto.Type));
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_ClientId_is_empty_then_validation_should_fail(string invalidString)
        {
            var dto = new AuthenticationDtoBuilder().With(x => x.ClientId, invalidString).Create();

            var result = _validator.Validate(dto);

            result.AssertSingleFailure(nameof(AuthenticationDto.ClientId));
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_Uri_is_empty_then_validation_should_fail(string invalidString)
        {
            var dto = new AuthenticationDtoBuilder().With(x => x.Uri, invalidString).Create();

            var result = _validator.Validate(dto);

            result.AssertSingleFailure(nameof(AuthenticationDto.Uri));
        }

        [Theory, IsUnit]
        [ClassData(typeof(InvalidUris))]
        public void When_Uri_is_invalid_then_validation_should_fail(string invalidString)
        {
            var dto = new AuthenticationDtoBuilder().With(x => x.Uri, invalidString).Create();

            var result = _validator.Validate(dto);

            result.AssertSingleFailure(nameof(AuthenticationDto.Uri));
        }

        [Fact, IsUnit]
        public void When_Scopes_are_empty_then_validation_should_fail()
        {
            var dto = new AuthenticationDtoBuilder().With(x => x.Scopes, null).Create();

            var result = _validator.Validate(dto);

            result.AssertSingleFailure(nameof(AuthenticationDto.Scopes));
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_Scope_contains_empty_item_then_validation_should_fail(string invalidString)
        {
            var dto = new AuthenticationDtoBuilder().With(x => x.Scopes, new List<string> { invalidString }).Create();

            var result = _validator.Validate(dto);

            result.AssertSingleFailure($"{nameof(AuthenticationDto.Scopes)}[0]");
        }

        [Fact, IsUnit]
        public void When_ClientSecret_is_null_then_validation_should_fail()
        {
            var dto = new AuthenticationDtoBuilder().With(x => x.ClientSecret, null).Create();

            var result = _validator.Validate(dto);

            result.AssertSingleFailure(nameof(AuthenticationDto.ClientSecret));
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_ClientSecretName_is_empty_item_then_validation_should_fail(string invalidString)
        {
            var dto = new AuthenticationDtoBuilder()
                .With(x => x.ClientSecret, new ClientSecretDto { Name = invalidString, Vault = "vault" })
                .Create();

            var result = _validator.Validate(dto);

            result.AssertSingleFailure(nameof(ClientSecretDto.Name));
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_ClientSecretVault_is_empty_item_then_validation_should_fail(string invalidString)
        {
            var dto = new AuthenticationDtoBuilder()
                .With(x => x.ClientSecret, new ClientSecretDto { Name = "secret-name", Vault = invalidString })
                .Create();

            var result = _validator.Validate(dto);

            result.AssertSingleFailure(nameof(ClientSecretDto.Vault));
        }
    }
}