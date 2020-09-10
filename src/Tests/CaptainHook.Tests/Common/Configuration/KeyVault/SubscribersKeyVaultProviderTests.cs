using System.IO;
using System.Text;
using CaptainHook.Common.Configuration;
using CaptainHook.Domain.Errors;
using CaptainHook.TestsInfrastructure.Builders;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace CaptainHook.Tests.Common.Configuration.KeyVault
{
    public class SubscribersKeyVaultProviderTests
    {
        [Fact, IsUnit]
        public void Load_WhenEverythingIsOk_ReturnValidSubscriberConfiguration()
        {
            var configurationData = @"{""event"":{
                ""1:type"": ""event1"",
                ""1:name"": ""event1"",
                ""1:webhookconfig:uri"": ""https://blah.blah.eshopworld.com"",
                ""1:webhookconfig:httpverb"": ""POST"",
                ""1:webhookconfig:authenticationconfig:type"": ""basic"",
                ""1:webhookconfig:authenticationconfig:username"": ""user"",
                ""1:webhookconfig:authenticationconfig:password"": ""password"",
                ""1:callbackconfig:name"": ""callback"",
                ""1:callbackconfig:type"": ""callback"",
                ""1:callbackconfig:httpverb"": ""POST"",
                ""1:callbackconfig:uri"": ""https://callback.eshopworld.com"",
                ""1:callbackconfig:webhookrequestrules:1:source:path"": ""$.TenantCode"",
                ""1:callbackconfig:webhookrequestrules:1:destination:ruleaction"": ""Route"",
                ""1:callbackconfig:webhookrequestrules:1:routes:1:uri"": ""https://callback.eshopworld.com/route1"",
                ""1:callbackconfig:webhookrequestrules:1:routes:1:selector"": ""testsel"",
                ""1:callbackconfig:webhookrequestrules:1:routes:1:authenticationconfig:type"": ""OIDC"",
                ""1:callbackconfig:webhookrequestrules:1:routes:1:authenticationconfig:uri"": ""https://blah-blah.sts.eshopworld.com"",
                ""1:callbackconfig:webhookrequestrules:1:routes:1:authenticationconfig:clientid"": ""ClientId"",
                ""1:callbackconfig:webhookrequestrules:1:routes:1:authenticationconfig:clientsecret"": ""ClientSecret"",
                ""1:callbackconfig:webhookrequestrules:1:routes:1:authenticationconfig:scopes:1"": ""scope1"",
                ""1:callbackconfig:webhookrequestrules:1:routes:1:authenticationconfig:scopes:2"": ""scope2"",
                ""1:callbackconfig:webhookrequestrules:1:routes:1:httpverb"": ""POST"",
            }}";

            var provider = new SubscribersKeyVaultProvider(CreateLoaderMock(configurationData));
            var result = provider.Load("keyvault");

            var expectedResult = new SubscriberConfigurationBuilder()
                .WithType("event1")
                .WithName(null)
                .WithCallback(callback => callback
                    .WithType("event1")
                    .WithName("callback")
                    .WithSubscriberName("callback")
                    .WithUri("https://callback.eshopworld.com")
                    .WithoutAuthentication()
                    .AddWebhookRequestRule(rule => rule
                        .WithSource(source => source.WithPath("$.TenantCode"))
                        .WithDestination(ruleAction: RuleAction.Route)
                        .AddRoute(route => route
                            .WithUri("https://callback.eshopworld.com/route1")
                            .WithSelector("testsel")
                            .WithHttpVerb("POST")
                            .WithOidcAuthentication()
                )))
                .AsMainConfiguration()
                .Create();

            result.IsError.Should().BeFalse();
            result.Data.Should().BeEquivalentTo(expectedResult);
        }

        [Fact, IsUnit]
        public void Load_WhenOidcDetailsAreMissing_ReturnError()
        {
            var configurationData = @"{""event"":{
                ""1:type"": ""event1"",
                ""1:name"": ""event1"",
                ""1:webhookconfig:uri"": ""https://blah.blah.eshopworld.com"",
                ""1:webhookconfig:httpverb"": ""POST"",
                ""1:webhookconfig:authenticationconfig:type"": ""OIDC""
            }}";

            var provider = new SubscribersKeyVaultProvider(CreateLoaderMock(configurationData));
            var result = provider.Load("keyvault");

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<KeyVaultConfigurationError>().Which.Failures.Should().HaveCount(1);
        }

        private static IKeyVaultConfigurationLoader CreateLoaderMock(string configurationData)
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(configurationData)));
            var configuration = builder.Build();

            var loader = new Mock<IKeyVaultConfigurationLoader>();
            loader.Setup(x => x.Load(It.IsAny<string>())).Returns(configuration);
            return loader.Object;
        }
    }
}