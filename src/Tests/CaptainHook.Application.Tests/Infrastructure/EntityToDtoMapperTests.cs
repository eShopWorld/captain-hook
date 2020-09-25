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
    public class EntityToDtoMapperTests
    {
        public static IEnumerable<object[]> PayloadTransformsMap =>
            new List<object[]>
            {
                new object[] { "$", null },
                new object[] { "$.Request", "Request" },
                new object[] { "$.Response", "Response" },
                new object[] { "$.OrderConfirmation", "OrderConfirmation" },
                new object[] { "$.PlatformOrderConfirmation", "PlatformOrderConfirmation" },
                new object[] { "$.EmptyCart", "EmptyCart" }
            };

        private readonly EntityToDtoMapper sut = new EntityToDtoMapper();

        [Fact]
        [IsUnit]
        public void MapAuthentication_When_BasicAuthenticationEntityIsUsed_Then_IsMappedToBasicAuthenticationDto()
        {
            // Arrange
            var entity = new BasicAuthenticationEntity("Username", "password-key-name");

            // Act
            var dto = sut.MapAuthentication(entity);

            // Assert
            using (new AssertionScope())
            {
                dto.Should().BeOfType(typeof(BasicAuthenticationDto));
                BasicAuthenticationDto basicDto = (BasicAuthenticationDto)dto;
                basicDto.Username.Should().Be("Username");
                basicDto.PasswordKeyName.Should().Be("password-key-name");
            }
        }

        [Fact]
        [IsUnit]
        public void MapAuthentication_When_OidcAuthenticationEntityIsUsed_Then_IsMappedToOidcAuthenticationDto()
        {
            // Arrange
            var entity = new OidcAuthenticationEntity(
                clientId: "client-id",
                clientSecretKeyName: "client-secret-name",
                uri: "http://sample.uri/auth",
                scopes: new string[] { "scope1", "scope2" });

            // Act
            var dto = sut.MapAuthentication(entity);

            // Assert
            using (new AssertionScope())
            {
                dto.Should().BeOfType(typeof(OidcAuthenticationDto));
                OidcAuthenticationDto dtoEntity = (OidcAuthenticationDto)dto;
                dtoEntity.ClientId.Should().Be("client-id");
                dtoEntity.ClientSecretKeyName.Should().Be("client-secret-name");
                dtoEntity.Uri.Should().Be("http://sample.uri/auth");
                dtoEntity.Scopes.Should().Contain(new List<string> { "scope1", "scope2" });
            }
        }

        [Fact]
        [IsUnit]
        public void MapUriTransform_When_ValidUriTransformEntityIsUsed_Then_IsMappedToUriTransformDto()
        {
            // Arrange
            var replacements = new Dictionary<string, string>
            {
                ["key1"] = "value1",
                ["key2"] = "value2"
            };

            var entity = new UriTransformEntity(replacements);

            // Act
            var dto = sut.MapUriTransform(entity);

            // Assert
            dto.Replace.Should().Contain(replacements.ToList());
        }

        [Theory]
        [IsUnit]
        [ClassData(typeof(ValidHttpVerbs))]
        public void MapEndpoint_When_ValidEndpointEntityIsUsed_Then_IsMappedToEndpointDto(string httpVerb)
        {
            // Arrange
            var entity = new EndpointEntity(
                uri: "http://www.test.url",
                authentication: new BasicAuthenticationEntity("username", "password-key-name"),
                httpVerb: httpVerb,
                selector: "Select");

            // Act
            var dto = sut.MapEndpoint(entity);

            // Assert
            using (new AssertionScope())
            {
                dto.Selector.Should().Be("Select");
                dto.Uri.Should().Be("http://www.test.url");
                dto.HttpVerb.Should().Be(httpVerb);
                dto.Authentication.Should().BeOfType(typeof(BasicAuthenticationDto));
                BasicAuthenticationDto authDto = (BasicAuthenticationDto)dto.Authentication;
                authDto.Username.Should().Be("username");
                authDto.PasswordKeyName.Should().Be("password-key-name");
            }
        }

        [Theory]
        [IsUnit]
        [ClassData(typeof(ValidHttpVerbs))]
        public void MapWebhooks_When_ValidSubscriberEntityIsUsed_Then_IsMappedToSubscriberDto(string httpVerb)
        {
            // Arrange
            var endpointEntity = new EndpointEntity(
                uri: "http://www.test.url",
                authentication: new BasicAuthenticationEntity("username", "password-key-name"),
                httpVerb: httpVerb,
                selector: "Select");

            var replacements = new Dictionary<string, string>
            {
                ["key1"] = "value1",
                ["key2"] = "value2"
            };

            var uriTransformEntity = new UriTransformEntity(replacements);

            var webhooksEntity = new WebhooksEntity(
                WebhooksEntityType.Webhooks,
                selectionRule: "$.selector",
                endpoints: new List<EndpointEntity> { endpointEntity },
                uriTransform: uriTransformEntity);

            var entity = new SubscriberEntity(string.Empty);
            entity.SetHooks(webhooksEntity);

            // Act
            var dto = sut.MapSubscriber(entity);

            // Assert
            using (new AssertionScope())
            {
                dto.Webhooks.SelectionRule.Should().Be("$.selector");
                dto.Webhooks.UriTransform.Replace.Should().Contain(replacements.ToList());
                dto.Webhooks.Endpoints.Should().HaveCount(1);
                var endpointDto = entity.Webhooks.Endpoints.First();
                endpointDto.Selector.Should().Be("Select");
                endpointDto.Uri.Should().Be("http://www.test.url");
                endpointDto.HttpVerb.Should().Be(httpVerb);
            }
        }
        
        [Theory]
        [MemberData(nameof(PayloadTransformsMap))]
        [IsUnit]
        public void MapWebhooks_When_MappingPayloadTransform_Then_IsMappedToCorrectDtoValue(string payloadTransformEntityValue, string expectedPayloadTransformDtoValue)
        {
            // Act
            var result = sut.MapPayloadTransform(payloadTransformEntityValue, WebhooksEntityType.Webhooks);

            // Assert
            result.Should().Be(expectedPayloadTransformDtoValue);
        }

        [Theory]
        [MemberData(nameof(PayloadTransformsMap))]
        [IsUnit]
        public void MapCallbacks_When_MappingPayloadTransform_Then_IsMappedToNull(string payloadTransformEntityValue, string _)
        {
            // Act
            var result = sut.MapPayloadTransform(payloadTransformEntityValue, WebhooksEntityType.Callbacks);

            // Assert
            result.Should().BeNull();
        }

        [Theory]
        [MemberData(nameof(PayloadTransformsMap))]
        [IsUnit]
        public void MapDlqhooks_When_MappingPayloadTransform_Then_IsMappedToCorrectDtoValue(string payloadTransformEntityValue, string expectedPayloadTransformDtoValue)
        {
            // Act
            var result = sut.MapPayloadTransform(payloadTransformEntityValue, WebhooksEntityType.DlqHooks);

            // Assert
            result.Should().Be(expectedPayloadTransformDtoValue);
        }
    }
}
