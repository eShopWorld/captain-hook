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
    public class DtoToEntityMapperTests
    {
        [Fact]
        [IsUnit]
        public void MapAuthentication_When_BasicAuthenticationDtoIsUsed_Then_IsMappedToBasicAuthenticationEntity()
        {
            var sut = new DtoToEntityMapper();

            var dto = new BasicAuthenticationDtoBuilder()
                .With(x => x.Username, "Username")
                .With(x => x.PasswordKeyName, "Password")
                .Create();

            var entity = sut.MapAuthentication(dto);

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
            var sut = new DtoToEntityMapper();

            var dto = new OidcAuthenticationDtoBuilder()
                .With(x => x.ClientId, "ClientId")
                .With(x => x.ClientSecretKeyName, "ClientSecretKeyName")
                .With(x => x.Uri, "http://security.uri")
                .With(x => x.Scopes, new List<string> { "scope" })
                .Create();

            var entity = sut.MapAuthentication(dto);

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
        public void MapUriTransform_When_ValidUriTransformDtoIsUsed_Then_IsMappedToUriTransformEntity()
        {
            var sut = new DtoToEntityMapper();

            var dictionary = new Dictionary<string, string>
            {
                ["key1"] = "value1",
                ["key2"] = "value2"
            };
            var dto = new UriTransformDtoBuilder()
                .With(x => x.Replace, dictionary)
                .Create();

            var entity = sut.MapUriTransform(dto);

            entity.Replace.Should().Contain(dictionary.ToList());
        }

        [Theory]
        [InlineData("POST")]
        [InlineData("PUT")]
        [InlineData("GET")]
        [IsUnit]
        public void MapEndpoint_When_ValidEndpointDtoIsUsed_Then_IsMappedToEndpointEntity(string httpVerb)
        {
            var sut = new DtoToEntityMapper();

            var dto = new EndpointDtoBuilder()
                .With(x => x.Selector, "Select")
                .With(x => x.HttpVerb, httpVerb)
                .With(x => x.Uri, "http://www.test.url")
                .Create();

            var entity = sut.MapEndpoint(dto, null);

            using (new AssertionScope())
            {
                entity.Selector.Should().Be("Select");
                entity.Uri.Should().Be("http://www.test.url");
                entity.HttpVerb.Should().Be(httpVerb);
            }
        }

        [Fact, IsUnit]
        public void MapEndpoint_When_ValidEndpointDtoIsUsedWithSelectorProvided_Then_IsMappedToEndpointEntity()
        {
            var sut = new DtoToEntityMapper();

            var dto = new EndpointDtoBuilder()
                .With(x => x.Selector, "Select")
                .With(x => x.HttpVerb, "PUT")
                .With(x => x.Uri, "http://www.test.url")
                .With(x => x.Authentication, null)
                .Create();

            var entity = sut.MapEndpoint(dto, "abc");

            var expectedEntity = new EndpointDto
            {
                Selector = "abc",
                Uri = "http://www.test.url",
                HttpVerb = "PUT"
            };
            entity.Should().BeEquivalentTo(expectedEntity);
        }

        [Theory]
        [InlineData("POST")]
        [InlineData("PUT")]
        [InlineData("GET")]
        [IsUnit]
        public void MapWebooks_When_ValidEndpointIsUsed_Then_IsMappedToEndpointEntity(string httpVerb)
        {
            var sut = new DtoToEntityMapper();

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

            var entity = sut.MapWebooks(dto, WebhooksEntityType.Webhooks);

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
    }
}
