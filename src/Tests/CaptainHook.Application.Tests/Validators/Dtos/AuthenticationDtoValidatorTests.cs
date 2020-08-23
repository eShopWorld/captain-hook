using System.Collections.Generic;
using CaptainHook.Application.Validators.Dtos;
using CaptainHook.Contract;
using CaptainHook.TestsInfrastructure.Builders;
using CaptainHook.TestsInfrastructure.TestsData;
using Eshopworld.Tests.Core;
using FluentValidation.TestHelper;
using Xunit;

namespace CaptainHook.Application.Tests.Validators.Dtos
{
    public class AuthenticationDtoValidatorTests
    {
        private readonly OidcAuthenticationValidator _oidcValidator = new OidcAuthenticationValidator();
        private readonly BasicAuthenticationValidator _basicValidator = new BasicAuthenticationValidator();

        [Fact, IsUnit]
        public void When_RequestIsValidOidc_Then_NoFailuresReturned()
        {
            var dto = new OidcAuthenticationDtoBuilder().Create();

            var result = _oidcValidator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact, IsUnit]
        public void When_RequestIsValidBasic_Then_NoFailuresReturned()
        {
            var dto = new BasicAuthenticationDtoBuilder().Create();

            var result = _basicValidator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_ClientIdIsEmptyForOidc_Then_ValidationFails(string invalidString)
        {
            var dto = new OidcAuthenticationDtoBuilder().With(x => x.ClientId, invalidString).Create();

            var result = _oidcValidator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ClientId);
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_UriIsEmptyForOidc_Then_ValidationFails(string invalidString)
        {
            var dto = new OidcAuthenticationDtoBuilder().With(x => x.Uri, invalidString).Create();

            var result = _oidcValidator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Uri);
        }

        [Theory, IsUnit]
        [ClassData(typeof(InvalidUris))]
        public void When_UriIsInvalidForOidc_Then_ValidationFails(string invalidString)
        {
            var dto = new OidcAuthenticationDtoBuilder().With(x => x.Uri, invalidString).Create();

            var result = _oidcValidator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Uri);
        }

        [Fact, IsUnit]
        public void When_ScopesIsEmptyForOidc_Then_ValidationFails()
        {
            var dto = new OidcAuthenticationDtoBuilder().With(x => x.Scopes, null).Create();

            var result = _oidcValidator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Scopes);
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_ScopesContainsSingleEmptyItemForOidc_Then_ValidationFails(string invalidString)
        {
            var dto = new OidcAuthenticationDtoBuilder().With(x => x.Scopes, new List<string> { invalidString }).Create();

            var result = _oidcValidator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Scopes);
        }

        [Fact, IsUnit]
        public void When_ClientSecretKeyIsNullForOidc_Then_ValidationFails()
        {
            var dto = new OidcAuthenticationDtoBuilder().With(x => x.ClientSecretKeyName, null).Create();

            var result = _oidcValidator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ClientSecretKeyName);
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_ClientSecretKeyNameIsEmptyForOidc_Then_ValidationFails(string invalidString)
        {
            var dto = new OidcAuthenticationDtoBuilder()
                .With(x => x.ClientSecretKeyName, invalidString)
                .Create();

            var result = _oidcValidator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ClientSecretKeyName);
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_UsernameIsEmptyForBasic_Then_ValidationFails(string invalidString)
        {
            var dto = new BasicAuthenticationDtoBuilder()
                .With(x => x.Username, invalidString)
                .Create();

            var result = _basicValidator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Username);
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_PasswordIsEmptyForBasic_Then_ValidationFails(string invalidString)
        {
            var dto = new BasicAuthenticationDtoBuilder()
                .With(x => x.Password, invalidString)
                .Create();

            var result = _basicValidator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Password);
        }
    }
}