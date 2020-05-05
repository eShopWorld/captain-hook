﻿using System;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Json;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace CaptainHook.Tests.Json
{
    public class AuthenticationConfigConverterTests
    {
        class TestObject
        {
            public string Name { get; set; }
            public AuthenticationConfig AuthenticationConfig { get; set; }
        }

        [Fact]
        [IsLayer0]
        public void OidcAuthentication_ShouldBeDeserializedProperly()
        {
            string data = @"{
                ""Name"": ""name"",
                ""AuthenticationConfig"": {
                  ""Type"": ""OIDC"",
                  ""ClientId"": ""clientid"",
                  ""Uri"": ""https://security.site.com/connect/token"",
                  ""ClientSecret"": ""verylongsecuresecret"",
                  ""Scopes"": [
                    ""t.abc.client.api.all""
                  ]
                }
            }";

            var testObject = JsonConvert.DeserializeObject<TestObject>(data, new AuthenticationConfigConverter());

            testObject.AuthenticationConfig.Type.Should().Be(AuthenticationType.OIDC);
            testObject.AuthenticationConfig.Should().BeOfType<OidcAuthenticationConfig>();
            var oidcAuth = (OidcAuthenticationConfig)testObject.AuthenticationConfig;
            oidcAuth.ClientId.Should().Be("clientid");
            oidcAuth.ClientSecret.Should().Be("verylongsecuresecret");
            oidcAuth.Uri.Should().Be("https://security.site.com/connect/token");
            oidcAuth.Scopes.Should().BeEquivalentTo("t.abc.client.api.all");
        }

        [Fact]
        [IsLayer0]
        public void BasicAuthentication_ShouldBeDeserializedProperly()
        {
            string data = @"{
                ""Name"": ""name"",
                ""AuthenticationConfig"": {
                  ""Type"": ""Basic"",
                  ""Username"": ""test name"",
                  ""Password"": ""verylongsecuresecret""
                }
            }";

            var testObject = JsonConvert.DeserializeObject<TestObject>(data, new AuthenticationConfigConverter());

            testObject.AuthenticationConfig.Type.Should().Be(AuthenticationType.Basic);
            testObject.AuthenticationConfig.Should().BeOfType<BasicAuthenticationConfig>();
            var basicAuth = (BasicAuthenticationConfig)testObject.AuthenticationConfig;
            basicAuth.Username.Should().Be("test name");
            basicAuth.Password.Should().Be("verylongsecuresecret");
        }

        [Fact]
        [IsLayer0]
        public void NoneAuthentication_ShouldBeDeserializedProperly()
        {
            string data = @"{
                ""Name"": ""name"",
                ""AuthenticationConfig"": {
                  ""Type"": ""None"",
                }
            }";

            var testObject = JsonConvert.DeserializeObject<TestObject>(data, new AuthenticationConfigConverter());

            testObject.AuthenticationConfig.Type.Should().Be(AuthenticationType.None);
            testObject.AuthenticationConfig.Should().BeOfType<AuthenticationConfig>();
        }

        [Fact]
        [IsLayer0]
        public void CustomAuthentication_ShouldBeDeserializedProperly()
        {
            string data = @"{
                ""Name"": ""name"",
                ""AuthenticationConfig"": {
                  ""Type"": ""Custom"",
                }
            }";

            var testObject = JsonConvert.DeserializeObject<TestObject>(data, new AuthenticationConfigConverter());

            testObject.AuthenticationConfig.Type.Should().Be(AuthenticationType.Custom);
            testObject.AuthenticationConfig.Should().BeOfType<AuthenticationConfig>();
        }

        [Fact]
        [IsLayer0]
        public void UnknownAuthentication_ShouldThrowAnException()
        {
            string data = @"{
                ""Name"": ""name"",
                ""AuthenticationConfig"": {
                  ""Type"": ""abc"",
                }
            }";

            Action act = () => JsonConvert.DeserializeObject<TestObject>(data, new AuthenticationConfigConverter());

            act.Should().Throw<JsonSerializationException>().WithMessage("Unknown authentication type");
        }
    }
}