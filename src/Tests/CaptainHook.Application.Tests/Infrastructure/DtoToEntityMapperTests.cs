﻿using CaptainHook.Application.Infrastructure.Mappers;
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
        public void MapAuthentication_WhenBasicAuthenticationDtoIsUsed_ThenIsMappedToBasicAuthenticationEntity()
        {
            var sut = new DtoToEntityMapper();

            var dto = new BasicAuthenticationDtoBuilder()
                .With(x => x.Username, "Username")
                .With(x => x.Password, "Password")
                .Create();

            var entity = sut.MapAuthentication(dto);

            using (new AssertionScope())
            {
                entity.Should().BeOfType(typeof(BasicAuthenticationEntity));
                BasicAuthenticationEntity basicEntity = (BasicAuthenticationEntity)entity;
                basicEntity.Username.Should().Be("Username");
                basicEntity.Password.Should().Be("Password");
            }
        }

        [Fact]
        [IsUnit]
        public void MapAuthentication_WhenOidcAuthenticationDtoIsUsed_ThenIsMappedToOidcAuthenticationEntity()
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
        public void MapUriTransform_WhenValidUriTransformIsUsed_ThenIsMappedToUriTransformEntity()
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
        public void MapEndpoint_WhenValidEndpointIsUsed_ThenIsMappedToEndpointEntity(string httpVerb)
        {
            var sut = new DtoToEntityMapper();

            var dto = new EndpointDtoBuilder()
                .With(x => x.Selector, "Select")
                .With(x => x.HttpVerb, httpVerb)
                .With(x => x.Uri, "http://www.test.url")
                .Create();


            var entity = sut.MapEndpoint(dto);

            using (new AssertionScope())
            {
                entity.Selector.Should().Be("Select");
                entity.Uri.Should().Be("http://www.test.url");
                entity.HttpVerb.Should().Be(httpVerb);
            }
        }

        [Theory]
        [InlineData("POST")]
        [InlineData("PUT")]
        [InlineData("GET")]
        [IsUnit]
        public void MapAEndpoint_WhenValidEndpointIsUsed_ThenIsMappedToEndpointEntity(string httpVerb)
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

            var entity = sut.MapWebooks(dto);

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
