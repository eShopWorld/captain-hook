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

namespace CaptainHook.Application.Tests.Infrastructure
{
    public class EntityToDtoMapperTests
    {
        private readonly EntityToDtoMapper sut = new EntityToDtoMapper();

        [Fact]
        [IsUnit]
        public void MapAuthentication_When_BasicAuthenticationEntityIsUsed_Then_IsMappedToBasicAuthenticationDto()
        {
            var entity = new BasicAuthenticationEntity("Username", "password-key-name");

            var dto = sut.MapAuthentication(entity);

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
            var entity = new OidcAuthenticationEntity(
                clientId: "client-id",
                clientSecretKeyName: "client-secret-name",
                uri: "http://sample.uri/auth",
                scopes: new string[] { "scope1", "scope2" });

            var dto = sut.MapAuthentication(entity);

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
            var replacements = new Dictionary<string, string>
            {
                ["key1"] = "value1",
                ["key2"] = "value2"
            };

            var entity = new UriTransformEntity(replacements);

            var dto = sut.MapUriTransform(entity);

            dto.Replace.Should().Contain(replacements.ToList());
        }

        [Theory]
        [InlineData("POST")]
        [InlineData("PUT")]
        [InlineData("GET")]
        [IsUnit]
        public void MapEndpoint_When_ValidEndpointEntityIsUsed_Then_IsMappedToEndpointDto(string httpVerb)
        {
            var entity = new EndpointEntity(
                uri: "http://www.test.url",
                authentication: new BasicAuthenticationEntity("username", "password-key-name"),
                httpVerb: httpVerb,
                selector: "Select");

            var dto = sut.MapEndpoint(entity);

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
        [InlineData("POST")]
        [InlineData("PUT")]
        [InlineData("GET")]
        [IsUnit]
        public void MapWebhooks_When_ValidSubscriberEntityIsUsed_Then_IsMappedToSubscriberDto(string httpVerb)
        {
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

            var dto = sut.MapSubscriber(entity);

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
    }
}
