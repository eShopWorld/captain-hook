using System;
using CaptainHook.Application.Infrastructure.Mappers;
using CaptainHook.TestsInfrastructure.Builders;
using FluentAssertions.Execution;
using CaptainHook.Domain.Entities;
using Eshopworld.Tests.Core;
using Xunit;
using FluentAssertions;
using System.Collections.Generic;
using CaptainHook.Contract;
using System.Linq;
using CaptainHook.TestsInfrastructure.TestsData;

namespace CaptainHook.Application.Tests.Infrastructure
{
    public class DtoToEntityMapperTests
    {
        public static IEnumerable<object[]> ValidPayloadTransforms =>
            new List<object[]>
            {
                new object[] { null, "$" },
                new object[] { "", "$" },
                new object[] { "Request", "$.Request" },
                new object[] { "request", "$.Request" },
                new object[] { "REQUEST", "$.Request" },
                new object[] { "Response", "$.Response" },
                new object[] { "response", "$.Response" },
                new object[] { "RESPONSE", "$.Response" },
                new object[] { "OrderConfirmation", "$.OrderConfirmation" },
                new object[] { "orderconfirmation", "$.OrderConfirmation" },
                new object[] { "ORDERCONFIRMATION", "$.OrderConfirmation" },
                new object[] { "PlatformOrderConfirmation", "$.PlatformOrderConfirmation" },
                new object[] { "platformorderconfirmation", "$.PlatformOrderConfirmation" },
                new object[] { "PLATFORMORDERCONFIRMATION", "$.PlatformOrderConfirmation" },
                new object[] { "EmptyCart", "$.EmptyCart" },
                new object[] { "emptycart", "$.EmptyCart" },
                new object[] { "EMPTYCART", "$.EmptyCart" },
            };

        public static IEnumerable<object[]> InvalidPayloadTransforms =>
            new List<object[]>
            {
                new object[] { "$" },
                new object[] { "$.Request" },
                new object[] { "$.Response" },
                new object[] { "$.OrderConfirmation" },
                new object[] { "$.PlatformOrderConfirmation" },
                new object[] { "$.EmptyCart" },
                new object[] { "AnyOtherCrazyString" },
                new object[] { 0 },
            };

        private readonly DtoToEntityMapper sut = new DtoToEntityMapper();

        [Fact]
        [IsUnit]
        public void MapAuthentication_When_BasicAuthenticationDtoIsUsed_Then_IsMappedToBasicAuthenticationEntity()
        {
            // Arrange
            var dto = new BasicAuthenticationDtoBuilder()
                .With(x => x.Username, "Username")
                .With(x => x.PasswordKeyName, "Password")
                .Create();

            // Act
            var entity = sut.MapAuthentication(dto);

            // Assert
            using (new AssertionScope())
            {
                entity.Should().BeOfType(typeof(BasicAuthenticationEntity));
                BasicAuthenticationEntity basicEntity = (BasicAuthenticationEntity)entity;
                basicEntity.Username.Should().Be("Username");
                basicEntity.PasswordKeyName.Should().Be("Password");
            }
        }

        [Fact]
        [IsUnit]
        public void MapAuthentication_When_OidcAuthenticationDtoIsUsed_Then_IsMappedToOidcAuthenticationEntity()
        {
            // Arrange
            var dto = new OidcAuthenticationDtoBuilder()
                .With(x => x.ClientId, "ClientId")
                .With(x => x.ClientSecretKeyName, "ClientSecretKeyName")
                .With(x => x.Uri, "http://security.uri")
                .With(x => x.Scopes, new List<string> { "scope" })
                .Create();

            // Act
            var entity = sut.MapAuthentication(dto);

            // Assert
            using (new AssertionScope())
            {
                entity.Should().BeOfType(typeof(OidcAuthenticationEntity));
                OidcAuthenticationEntity oidcEntity = (OidcAuthenticationEntity)entity;
                oidcEntity.ClientId.Should().Be("ClientId");
                oidcEntity.ClientSecretKeyName.Should().Be("ClientSecretKeyName");
                oidcEntity.Uri.Should().Be("http://security.uri");
                oidcEntity.Scopes.Should().Contain(new List<string> { "scope" });
            }
        }

        [Fact]
        [IsUnit]
        public void MapAuthentication_When_NoAuthenticationDtoIsUsed_Then_IsMappedToNull()
        {
            // Arrange
            var dto = new NoAuthenticationDto();

            // Act
            var entity = sut.MapAuthentication(dto);

            // Assert
            entity.Should().BeNull();
        }

        [Fact]
        [IsUnit]
        public void MapAuthentication_When_NullIsUsed_Then_IsMappedToNull()
        {
            // Act
            var entity = sut.MapAuthentication(null);

            // Assert
            entity.Should().BeNull();
        }

        [Fact]
        [IsUnit]
        public void MapUriTransform_When_ValidUriTransformDtoIsUsed_Then_IsMappedToUriTransformEntity()
        {
            // Arrange
            var dictionary = new Dictionary<string, string>
            {
                ["key1"] = "value1",
                ["key2"] = "value2"
            };
            var dto = new UriTransformDtoBuilder()
                .With(x => x.Replace, dictionary)
                .Create();

            // Act
            var entity = sut.MapUriTransform(dto);

            // Assert
            entity.Replace.Should().Contain(dictionary.ToList());
        }

        [Theory]
        [IsUnit]
        [ClassData(typeof(ValidHttpVerbs))]
        public void MapEndpoint_When_ValidEndpointDtoIsUsed_Then_IsMappedToEndpointEntity(string httpVerb)
        {
            // Arrange
            var dto = new EndpointDtoBuilder()
                .With(x => x.Selector, "Select")
                .With(x => x.HttpVerb, httpVerb)
                .With(x => x.Uri, "http://www.test.url")
                .Create();

            // Act
            var entity = sut.MapEndpoint(dto, null);

            // Assert
            using (new AssertionScope())
            {
                entity.Selector.Should().Be("Select");
                entity.Uri.Should().Be("http://www.test.url");
                entity.HttpVerb.Should().Be(httpVerb);
                entity.Timeout.Should().BeNull();
                entity.RetrySleepDurations.Should().BeNull();
            }
        }

        [Fact]
        [IsUnit]
        public void MapEndpoint_When_TimeoutAndRetriesProvided_Then_AreMappedToEndpointEntity()
        {
            // Arrange
            var dto = new EndpointDtoBuilder()
                .With(x => x.HttpVerb, "PUT")
                .With(x => x.Uri, "http://www.test.url")
                .With(x => x.Timeout, TimeSpan.FromSeconds(10))
                .With(x => x.RetrySleepDurations, new[] { TimeSpan.FromSeconds(1) })
                .Create();

            // Act
            var entity = sut.MapEndpoint(dto, null);

            // Assert
            using (new AssertionScope())
            {
                entity.Timeout.Should().Be(TimeSpan.FromSeconds(10));
                entity.RetrySleepDurations.Should().BeEquivalentTo(new[] { TimeSpan.FromSeconds(1) });
            }
        }

        [Fact]
        [IsUnit]
        public void MapEndpoint_When_ValidEndpointDtoIsUsedWithSelectorProvided_Then_IsMappedToEndpointEntity()
        {
            // Arrange
            var dto = new EndpointDtoBuilder()
                .With(x => x.Selector, "Select")
                .With(x => x.HttpVerb, "PUT")
                .With(x => x.Uri, "http://www.test.url")
                .With(x => x.Authentication, new NoAuthenticationDto())
                .Create();

            // Act
            var entity = sut.MapEndpoint(dto, "abc");

            // Assert
            var expectedEntity = new EndpointEntity("http://www.test.url", null, "PUT", "abc");
            entity.Should().BeEquivalentTo(expectedEntity);
        }

        [Theory]
        [IsUnit]
        [ClassData(typeof(ValidHttpVerbs))]
        public void MapWebooks_When_ValidEndpointIsUsed_Then_IsMappedToEndpointEntity(string httpVerb)
        {
            // Arrange
            var endpointDto = new EndpointDtoBuilder()
                .With(x => x.Selector, "Select")
                .With(x => x.HttpVerb, httpVerb)
                .With(x => x.Uri, "http://www.test.url")
                .Create();

            var dictionary = new Dictionary<string, string>
            {
                ["key1"] = "value1",
                ["key2"] = "value2"
            };
            var uriTransformDto = new UriTransformDtoBuilder()
                .With(x => x.Replace, dictionary)
                .Create();

            var dto = new WebhooksDtoBuilder()
                .With(x => x.SelectionRule, "$.Select")
                .With(x => x.Endpoints, new List<EndpointDto> { endpointDto })
                .With(x => x.UriTransform, uriTransformDto)
                .Create();

            // Act
            var entity = sut.MapWebooks(dto, WebhooksEntityType.Webhooks);

            // Assert
            using (new AssertionScope())
            {
                entity.SelectionRule.Should().Be("$.Select");
                entity.Endpoints.Should().HaveCount(1);
                var endpointEntity = entity.Endpoints.First();
                endpointEntity.Selector.Should().Be("Select");
                endpointEntity.Uri.Should().Be("http://www.test.url");
                endpointEntity.HttpVerb.Should().Be(httpVerb);
                entity.UriTransform.Replace.Should().Contain(dictionary.ToList());
            }
        }

        [Theory]
        [IsUnit]
        [MemberData(nameof(ValidPayloadTransforms))]
        public void MapWebhooks_When_ValidPayloadTransformIsUsed_Then_IsMappedCorrectEntityValue(string payloadTransform, string expectedPayloadTransform)
        {
            // Act
            var result = sut.MapPayloadTransform(payloadTransform, WebhooksEntityType.Webhooks);

            // Assert
            result.Should().Be(expectedPayloadTransform);
        }

        [Theory]
        [IsUnit]
        [MemberData(nameof(ValidPayloadTransforms))]
        public void MapCallbacks_When_ValidPayloadTransformIsUsed_Then_IsMappedToNull(string payloadTransform, string _)
        {
            // Act
            var result = sut.MapPayloadTransform(payloadTransform, WebhooksEntityType.Callbacks);

            // Assert
            result.Should().BeNull();
        }

        [Theory]
        [IsUnit]
        [MemberData(nameof(ValidPayloadTransforms))]
        public void MapDlqhooks_When_ValidPayloadTransformIsUsed_Then_IsMappedCorrectEntityValue(string payloadTransform, string expectedPayloadTransform)
        {
            // Act
            var result = sut.MapPayloadTransform(payloadTransform, WebhooksEntityType.DlqHooks);

            // Assert
            result.Should().Be(expectedPayloadTransform);
        }

        [Theory]
        [IsUnit]
        [MemberData(nameof(InvalidPayloadTransforms))]
        public void MapWebhooks_When_InvalidPayloadTransformIsUsed_Then_IsMappedToDefault(string payloadTransform)
        {
            // Act
            var result = sut.MapPayloadTransform(payloadTransform, WebhooksEntityType.Webhooks);

            // Assert
            result.Should().Be("$");
        }

        [Theory]
        [IsUnit]
        [MemberData(nameof(InvalidPayloadTransforms))]
        public void MapCallbacks_When_InvalidPayloadTransformIsUsed_Then_IsMappedToNull(string payloadTransform)
        {
            // Act
            var result = sut.MapPayloadTransform(payloadTransform, WebhooksEntityType.Callbacks);

            // Assert
            result.Should().BeNull();
        }

        [Theory]
        [IsUnit]
        [MemberData(nameof(InvalidPayloadTransforms))]
        public void MapDlqhooks_When_InvalidPayloadTransformIsUsed_Then_IsMappedToDefault(string payloadTransform)
        {
            // Act
            var result = sut.MapPayloadTransform(payloadTransform, WebhooksEntityType.DlqHooks);

            // Assert
            result.Should().Be("$");
        }
    }
}
