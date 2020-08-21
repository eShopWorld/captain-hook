using System.Collections.Generic;
using CaptainHook.Application.Validators.Dtos;
using CaptainHook.Contract;
using CaptainHook.TestsInfrastructure.Builders;
using CaptainHook.TestsInfrastructure.TestsData;
using Eshopworld.Tests.Core;
using FluentValidation.TestHelper;
using Xunit;

namespace CaptainHook.Application.Tests.RequestValidators
{
    public class AuthenticationDtoValidatorTests
    {
        private readonly AuthenticationDtoValidator<OidcAuthenticationDto> _validator = new AuthenticationDtoValidator<OidcAuthenticationDto>();

        [Fact, IsUnit]
        public void When_RequestIsValid_Then_NoFailuresReturned()
        {
            var dto = new AuthenticationDtoBuilder().Create();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact, IsUnit]
        public void When_TypeIsLowercase_Then_NoFailuresReturned()
        {
            var dto = new AuthenticationDtoBuilder().With(x => x.Type, "oidc").Create();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_TypeIsEmpty_Then_ValidationFails(string invalidString)
        {
            var dto = new AuthenticationDtoBuilder().With(x => x.Type, invalidString).Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Type);
        }

        [Theory, IsUnit]
        [InlineData("unknown type")]
        public void When_TypeIsUnknown_Then_ValidationFails(string invalidString)
        {
            var dto = new AuthenticationDtoBuilder().With(x => x.Type, invalidString).Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Type);
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_ClientIdIsEmpty_Then_ValidationFails(string invalidString)
        {
            var dto = new AuthenticationDtoBuilder().With(x => x.ClientId, invalidString).Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ClientId);
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_UriIsEmpty_Then_ValidationFails(string invalidString)
        {
            var dto = new AuthenticationDtoBuilder().With(x => x.Uri, invalidString).Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Uri);
        }

        [Theory, IsUnit]
        [ClassData(typeof(InvalidUris))]
        public void When_UriIsInvalid_Then_ValidationFails(string invalidString)
        {
            var dto = new AuthenticationDtoBuilder().With(x => x.Uri, invalidString).Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Uri);
        }

        [Fact, IsUnit]
        public void When_ScopesIsEmpty_Then_ValidationFails()
        {
            var dto = new AuthenticationDtoBuilder().With(x => x.Scopes, null).Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Scopes);
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_ScopesContainsSingleEmptyItem_Then_ValidationFails(string invalidString)
        {
            var dto = new AuthenticationDtoBuilder().With(x => x.Scopes, new List<string> { invalidString }).Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Scopes);
        }

        [Fact, IsUnit]
        public void When_ClientSecretKeyIsNull_Then_ValidationFails()
        {
            var dto = new AuthenticationDtoBuilder().With(x => x.ClientSecretKeyName, null).Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ClientSecretKeyName);
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_ClientSecretKeyNameIsEmpty_Then_ValidationFails(string invalidString)
        {
            var dto = new AuthenticationDtoBuilder()
                .With(x => x.ClientSecretKeyName, invalidString)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ClientSecretKeyName);
        }
    }
}