using System;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Json;
using CaptainHook.Contract;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using Newtonsoft.Json;
using Xunit;

namespace CaptainHook.Tests.Json
{
    public class AuthenticationDtoJsonConverterTests
    {
        [Fact]
        [IsUnit]
        public void WhenAuthenticationIsOIDC_ThenItIsDeserializedProperly()
        {
            string data = @"{
                ""type"": ""OIDC"",
                ""clientId"": ""clientid"",
                ""uri"": ""https://security.site.com/connect/token"",
                ""ClientSecretKeyName"": ""secret--key--name"",
                ""scopes"": [
                ""t.abc.client.api.all""
                ]
            }";

            var result = JsonConvert.DeserializeObject<OidcAuthenticationDto>(data, new AuthenticationDtoJsonConverter());

            using (new AssertionScope())
            {
                result.AuthenticationType.Should().Be(OidcAuthenticationDto.Type);
                result.Should().BeOfType<OidcAuthenticationDto>();
                result.ClientId.Should().Be("clientid");
                result.ClientSecretKeyName.Should().Be("secret--key--name");
                result.Uri.Should().Be("https://security.site.com/connect/token");
                result.Scopes.Should().BeEquivalentTo("t.abc.client.api.all");
            }
        }

        [Fact]
        [IsUnit]
        public void WhenAuthenticationIsBasic_ThenItIsDeserializedProperly()
        {
            string data = @"{
                ""type"": ""Basic"",
                ""username"": ""chuck"",
                ""password"": ""norris""
            }";

            var result = JsonConvert.DeserializeObject<BasicAuthenticationDto>(data, new AuthenticationDtoJsonConverter());

            using (new AssertionScope())
            {
                result.AuthenticationType.Should().Be(BasicAuthenticationDto.Type);
                result.Should().BeOfType<BasicAuthenticationDto>();
                result.Username.Should().Be("chuck");
                result.Password.Should().Be("norris");
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
        public void WhenAuthenticationIsInvalid_ThenItIsDeserializedAsNull(string authType)
        {
            string data = $@"{{
                ""type"": ""{authType}"",
                ""username"": ""chuck"",
                ""password"": ""norris""
            }}";

            var result = JsonConvert.DeserializeObject<BasicAuthenticationDto>(data, new AuthenticationDtoJsonConverter());

            result.Should().BeNull();
        }
    }
}
