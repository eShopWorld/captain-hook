using System;
using System.Collections.Generic;
using System.Net.Http;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.EventHandlerActor.Handlers.Requests;
using Eshopworld.Core;
using Eshopworld.Tests.Core;
using Moq;
using Xunit;

namespace CaptainHook.Tests.Services.Actors.Requests
{
    public class RouteAndReplaceRequestBuilderTests
    {
        [IsUnit]
        [Theory]
        [MemberData(nameof(UriDataRouteAndReplace))]
        public void UriConstructionRouteAndReplaceTests(WebhookConfig config, string payload, string expectedUri)
        {
            var uri = new RouteAndReplaceRequestBuilder(Mock.Of<IBigBrother>()).BuildUri(config, payload);

            Assert.Equal(new Uri(expectedUri), uri);
        }

        public static IEnumerable<object[]> UriDataRouteAndReplace =>
            new List<object[]>
            {
                new object[]
                {
                    new WebhookConfig
                    {
                        Name = "Webhook1",
                        WebhookRequestRules = new List<WebhookRequestRule>
                        {
                            new WebhookRequestRule
                            {
                                Source = new SourceParserLocation
                                {
                                    Replace = new Dictionary<string, string>
                                    {
                                        { "selector",  "$.TenantCode"},
                                        { "orderCode",  "$.OrderCode"}
                                    }
                                },
                                Destination = new ParserLocation
                                {
                                    RuleAction = RuleAction.RouteAndReplace
                                },
                                Routes = new List<WebhookConfigRoute>
                                {
                                    new WebhookConfigRoute
                                    {
                                        Uri = "https://blah.blah.Brand1.eshopworld.com/webhook/{orderCode}",
                                        HttpMethod = HttpMethod.Post,
                                        Selector = "Brand1",
                                        AuthenticationConfig = new AuthenticationConfig
                                        {
                                            Type = AuthenticationType.None
                                        }
                                    },
                                    new WebhookConfigRoute
                                    {
                                        Uri = "https://blah.blah.{selector}.eshopworld.com/webhook/{orderCode}",
                                        HttpMethod = HttpMethod.Put,
                                        Selector = "*",
                                        AuthenticationConfig = new AuthenticationConfig
                                        {
                                            Type = AuthenticationType.None
                                        }
                                    }
                                }
                            }
                        }
                    },
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand2\", \"TenantCode\":\"tenant1\"}",
                    "https://blah.blah.tenant1.eshopworld.com/webhook/9744b831-df2c-4d59-9d9d-691f4121f73a"
                },
                new object[]
                {
                    new WebhookConfig
                    {
                        Name = "Webhook2",
                        WebhookRequestRules = new List<WebhookRequestRule>
                        {
                            new WebhookRequestRule
                            {
                                Source = new SourceParserLocation
                                {
                                    Replace = new Dictionary<string, string>
                                    {
                                        { "selector",  "$.TenantCode"},
                                        { "orderCode",  "$.OrderCode"}
                                    }
                                },
                                Destination = new ParserLocation
                                {
                                    RuleAction = RuleAction.RouteAndReplace
                                },
                                Routes = new List<WebhookConfigRoute>
                                {
                                    new WebhookConfigRoute
                                    {
                                        Uri = "https://blah.blah.{selector}.eshopworld.com/webhook/{orderCode}",
                                        HttpMethod = HttpMethod.Put,
                                        Selector = "*",
                                        AuthenticationConfig = new AuthenticationConfig
                                        {
                                            Type = AuthenticationType.None
                                        }
                                    }
                                }
                            }
                        }
                    },
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand2\", \"TenantCode\":\"tenant1\"}",
                    "https://blah.blah.tenant1.eshopworld.com/webhook/9744b831-df2c-4d59-9d9d-691f4121f73a"
                },
                new object[]
                {
                    new WebhookConfig
                    {
                        Name = "Webhook3",
                        WebhookRequestRules = new List<WebhookRequestRule>
                        {
                            new WebhookRequestRule
                            {
                                Source = new SourceParserLocation
                                {
                                    Replace = new Dictionary<string, string>
                                    {
                                        { "selector",  "$.TenantCode"},
                                        { "orderCode",  "$.OrderCode"}
                                    }
                                },
                                Destination = new ParserLocation
                                {
                                    RuleAction = RuleAction.RouteAndReplace
                                },
                                Routes = new List<WebhookConfigRoute>
                                {
                                    new WebhookConfigRoute
                                    {
                                        Uri = "https://blah.blah.{selector}.eshopworld.com/webhook/{orderCode}",
                                        HttpMethod = HttpMethod.Put,
                                        Selector = "*",
                                        AuthenticationConfig = new AuthenticationConfig
                                        {
                                            Type = AuthenticationType.None
                                        }
                                    }
                                }
                            }
                        }
                    },
                    "{\"OrderCode\":\"DEV13:00026804\", \"BrandType\":\"Brand2\", \"TenantCode\":\"tenant1\"}",
                    "https://blah.blah.tenant1.eshopworld.com/webhook/DEV13%3A00026804"
                }
            };
    }
}