﻿using CaptainHook.Storage.Cosmos.Models;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using Newtonsoft.Json;
using System;
using Xunit;

namespace CaptainHook.Storage.Cosmos.Tests
{
    public class AuthenticationSubdocumentJsonConverterTests
    {
        [Fact]
        [IsUnit]
        public void WhenAuthenticationIsOIDC_ThenItIsDeserializedProperly()
        {
            string data = @"{
                ""type"": ""OIDC"",
                ""clientId"": ""clientid"",
                ""uri"": ""https://security.site.com/connect/token"",
                ""secretName"": ""secret--key--name"",
                ""scopes"": [
                    ""t.abc.client.api.all""
                ]
            }";

            var result = JsonConvert.DeserializeObject<AuthenticationSubdocument>(data, new AuthenticationSubdocumentJsonConverter());

            using (new AssertionScope())
            {
                result.AuthenticationType.Should().Be(OidcAuthenticationSubdocument.Type);
                result.Should().BeOfType<OidcAuthenticationSubdocument>();

                var oidcAuth = result as OidcAuthenticationSubdocument;
                oidcAuth.ClientId.Should().Be("clientid");
                oidcAuth.SecretName.Should().Be("secret--key--name");
                oidcAuth.Uri.Should().Be("https://security.site.com/connect/token");
                oidcAuth.Scopes.Should().BeEquivalentTo("t.abc.client.api.all");
            }
        }

        [Fact]
        [IsUnit]
        public void WhenAuthenticationIsBasic_ThenItIsDeserializedProperly()
        {
            string data = @"{
                ""type"": ""Basic"",
                ""username"": ""chuck"",
                ""passwordKeyName"": ""norris""
            }";

            var result = JsonConvert.DeserializeObject<AuthenticationSubdocument>(data, new AuthenticationSubdocumentJsonConverter());

            using (new AssertionScope())
            {
                result.AuthenticationType.Should().Be(BasicAuthenticationSubdocument.Type);
                result.Should().BeOfType<BasicAuthenticationSubdocument>();
                
                var basicAuth = result as BasicAuthenticationSubdocument;
                basicAuth.Username.Should().Be("chuck");
                basicAuth.PasswordKeyName.Should().Be("norris");
            }
        }

        [Theory]
        [InlineData("0")]
        [InlineData("basic")]
        [InlineData("oidc")]
        [InlineData("custom")]
        [InlineData("none")]
        [InlineData("invalid")]
        [InlineData(null)]
        [InlineData("")]
        [IsUnit]
        public void WhenAuthenticationTypeIsInvalid_ThenItIsDeserializedAsNull(string authType)
        {
            string data = $@"{{
                ""type"": ""{authType}"",
                ""username"": ""chuck"",
                ""password"": ""norris""
            }}";

            var result = JsonConvert.DeserializeObject<AuthenticationSubdocument>(data, new AuthenticationSubdocumentJsonConverter());

            result.Should().BeNull();
        }

        [Theory]
        [InlineData("{ authentication: 0 }")]
        [InlineData("{ authentication: \"invalid\" }")]
        [InlineData("{ authentication: \"\" }")]
        [InlineData("{ authentication: {} }")]
        [InlineData("{ authentication: null }")]
        [IsUnit]
        public void WhenAuthenticationIsInvalid_ThenItIsDeserializedAsNull(string data)
        {
            var result = JsonConvert.DeserializeObject<AuthenticationWrapper>(data, new AuthenticationSubdocumentJsonConverter());

            result.Authentication.Should().BeNull();
        }

        internal class AuthenticationWrapper
        {
            public AuthenticationSubdocument Authentication { get; set; }
        }
    }
}
