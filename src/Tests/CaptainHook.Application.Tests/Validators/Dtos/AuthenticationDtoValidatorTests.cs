﻿using System.Collections.Generic;
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
        private readonly OidcAuthenticationDtoValidator _oidcValidator = new OidcAuthenticationDtoValidator();
        private readonly BasicAuthenticationDtoValidator _basicValidator = new BasicAuthenticationDtoValidator();
        private readonly EndpointDtoValidator _endpointDtoValidator = new EndpointDtoValidator();

        [Fact, IsUnit]
        public void When_RequestIsNoAuthentication_Then_NoFailuresReturned()
        {
            var authenticationDto = new NoAuthenticationDto();

            var dto = new EndpointDtoBuilder()
                .With(x => x.Authentication, authenticationDto)
                .Create();

            var result = _endpointDtoValidator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact, IsUnit]
        public void When_RequestIsInvalidAuthentication_Then_ValidationFails()
        {
            var authenticationDto = new InvalidAuthenticationDto();

            var dto = new EndpointDtoBuilder()
                .With(x => x.Authentication, authenticationDto)
                .Create();

            var result = _endpointDtoValidator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Authentication);
        }

        [Fact, IsUnit]
        public void When_RequestIsValidOidcWithScopes_Then_NoFailuresReturned()
        {
            var dto = new OidcAuthenticationDtoBuilder().Create();

            var result = _oidcValidator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        public static IEnumerable<object[]> EmptyLists =>
            new List<object[]>
            {
                new object[] {new List<string>()},
                new object[] {null},
            };

        [Theory, IsUnit]
        [MemberData(nameof(EmptyLists))]
        public void When_RequestIsValidOidcWithUseHeaders_Then_NoFailuresReturned(List<string> scopes)
        {
            var dto = new OidcAuthenticationDtoBuilder()
                .With(x => x.Scopes, scopes)
                .With(x => x.UseHeaders, true)
                .Create();

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
        [InlineData("https://test{template}url.com")]
        public void When_UriIsInvalidForOidc_Then_ValidationFails(string invalidString)
        {
            var dto = new OidcAuthenticationDtoBuilder().With(x => x.Uri, invalidString).Create();

            var result = _oidcValidator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Uri);
        }

        [Theory, IsUnit]
        [MemberData(nameof(EmptyLists))]
        public void When_ScopesIsEmptyAndUseHeadersIsFalseForOidc_Then_ValidationFails(List<string> scopes)
        {
            var dto = new OidcAuthenticationDtoBuilder()
                .With(x => x.Scopes, scopes)
                .With(x => x.UseHeaders, false)
                .Create();

            var result = _oidcValidator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Scopes);
        }

        [Fact, IsUnit]
        public void When_ScopesIsNotEmptyAndUseHeadersIsTrueForOidc_Then_ValidationFails()
        {
            var dto = new OidcAuthenticationDtoBuilder()
                .With(x => x.Scopes, new List<string> {"test.scope.api"})
                .With(x => x.UseHeaders, true)
                .Create();

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
                .With(x => x.PasswordKeyName, invalidString)
                .Create();

            var result = _basicValidator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.PasswordKeyName);
        }
    }
}