using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using CaptainHook.Cli.Commands.GeneratePowerShell;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Xunit;

namespace CaptainHook.Cli.Tests.GeneratePowerShell
{
    public class EventHandlerConfigToPowerShellConverterTests
    {
        private static async Task<IEnumerable<string>> CallConverter(params EventHandlerConfig[] eventHandlerConfig)
        {
            var result = await new EventHandlerConfigToPowerShellConverter().Convert(eventHandlerConfig);
            return result;
        }

        [Fact, IsLayer0]
        public async Task ShouldBuildEventHandlerTypeAndName()
        {
            var eventHandlerConfig = new EventHandlerConfig
            {
                Name = "name1",
                Type = "type1",
            };

            var result = await CallConverter(eventHandlerConfig);

            var expected = new[]
            {
                "setConfig 'event--1--name' 'name1' $KeyVault",
                "setConfig 'event--1--type' 'type1' $KeyVault",
            };

            result.Should().Contain(expected);
        }

        [Fact, IsLayer0]
        public async Task ShouldBuildEventHandlerTypeAndNameForMultipleEvents()
        {
            var eventHandlerConfig = new EventHandlerConfig
            {
                Name = "name1",
                Type = "type1",
            };
            var secondEventHandlerConfig = new EventHandlerConfig
            {
                Name = "name2",
                Type = "type2",
            };

            var result = await CallConverter(eventHandlerConfig, secondEventHandlerConfig);

            var expected = new[]
            {
                "setConfig 'event--1--name' 'name1' $KeyVault",
                "setConfig 'event--1--type' 'type1' $KeyVault",
                "setConfig 'event--2--name' 'name2' $KeyVault",
                "setConfig 'event--2--type' 'type2' $KeyVault",
            };

            result.Should().Contain(expected);
        }

        [Fact, IsLayer0]
        public async Task ShouldBuildWebHookConfigNameAndUrl()
        {
            var eventHandlerConfig = new EventHandlerConfig
            {
                WebhookConfig = new WebhookConfig
                {
                    Name = "webhook1-name",
                    Uri = "https://site.com/api/v1/WebHook1/",
                    HttpVerb = "POST"
                }
            };

            var result = await CallConverter(eventHandlerConfig);

            var expected = new[]
            {
                "setConfig 'event--1--webhookconfig--name' 'webhook1-name' $KeyVault",
                "setConfig 'event--1--webhookconfig--uri' 'https://site.com/api/v1/WebHook1/' $KeyVault",
                "setConfig 'event--1--webhookconfig--httpverb' 'POST' $KeyVault"
            };

            result.Should().Contain(expected);
        }

        [Fact, IsLayer0]
        public async Task ShouldBuildWebHookConfigOidcAuthenticationConfig()
        {
            var eventHandlerConfig = new EventHandlerConfig
            {
                WebhookConfig = new WebhookConfig
                {
                    AuthenticationConfig = new OidcAuthenticationConfig
                    {
                        Type = AuthenticationType.OIDC,
                        ClientId = "t.abc.client",
                        Uri = "https://security.site.com/connect/token",
                        ClientSecret = "verylongsecuresecret",
                        Scopes = new[]
                        {
                            "t.abc.client.api.all"
                        }
                    },
                }
            };

            var result = await CallConverter(eventHandlerConfig);

            var expected = new[]
            {
                "setConfig 'event--1--webhookconfig--authenticationconfig--type' 2 $KeyVault",
                "setConfig 'event--1--webhookconfig--authenticationconfig--uri' 'https://security.site.com/connect/token' $KeyVault",
                "setConfig 'event--1--webhookconfig--authenticationconfig--clientid' 't.abc.client' $KeyVault",
                "setConfig 'event--1--webhookconfig--authenticationconfig--clientsecret' 'verylongsecuresecret' $KeyVault",
                "setConfig 'event--1--webhookconfig--authenticationconfig--scopes' 't.abc.client.api.all' $KeyVault"
            };

            result.Should().Contain(expected);
        }

        [Fact, IsLayer0]
        public async Task ShouldBuildWebHookConfigWebhookRequestRule()
        {
            var eventHandlerConfig = new EventHandlerConfig
            {
                WebhookConfig = new WebhookConfig
                {
                    WebhookRequestRules = new List<WebhookRequestRule>
                    {
                        new WebhookRequestRule
                        {
                            Source = new ParserLocation
                            {
                                Path = "path1",
                                Type = DataType.Model
                            },
                            Destination = new ParserLocation
                            {
                                Type = DataType.Model
                            }
                        },
                        new WebhookRequestRule
                        {
                            Source = new ParserLocation
                            {
                                Path = "path2"
                            },
                            Destination = new ParserLocation
                            {
                                RuleAction = RuleAction.Route
                            },
                        }
                    }
                }
            };

            var result = await CallConverter(eventHandlerConfig);

            var expected = new[]
            {
                "setConfig 'event--1--webhookconfig--webhookrequestrules--1--source--path' 'path1' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--1--source--type' 'Model' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--1--destination--type' 'Model' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--source--path' 'path2' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--destination--ruleaction' 'route' $KeyVault"
            };

            result.Should().Contain(expected);
        }

        [Fact, IsLayer0]
        public async Task ShouldBuildWebHookConfigWebhookConfigRoutes()
        {
            var eventHandlerConfig = new EventHandlerConfig
            {
                WebhookConfig = new WebhookConfig
                {
                    WebhookRequestRules = new List<WebhookRequestRule>
                    {
                        new WebhookRequestRule
                        {
                        },
                        new WebhookRequestRule
                        {
                            Routes = new List<WebhookConfigRoute>
                            {
                                new WebhookConfigRoute
                                {
                                    Uri = "https://company1.com/api/",
                                    Selector = "selector1",
                                    HttpVerb = "POST",
                                    AuthenticationConfig = new OidcAuthenticationConfig
                                    {
                                        Type = AuthenticationType.OIDC,
                                        ClientId = "client1.test.client",
                                        Uri = "https://security.test.company1.com/connect/token",
                                        ClientSecret = "verylongandsecuresecret1",
                                        Scopes = new [] { "activity1.webhook.api1.all" }
                                    }
                                },
                                new WebhookConfigRoute
                                {
                                    Uri = "https://company2.com/api/",
                                    Selector = "selector2",
                                    HttpVerb = "POST",
                                    AuthenticationConfig = new OidcAuthenticationConfig
                                    {
                                        Type = AuthenticationType.OIDC,
                                        ClientId = "client2.test.client",
                                        Uri = "https://security.test.company2.com/connect/token",
                                        ClientSecret = "verylongandsecuresecret2",
                                        Scopes = new [] { "activity1.webhook.api2.all" }
                                    }
                                },
                                new WebhookConfigRoute
                                {
                                    Uri = "https://company3.com/api/",
                                    Selector = "selector3",
                                    HttpVerb = "POST",
                                    AuthenticationConfig = new OidcAuthenticationConfig
                                    {
                                        Type = AuthenticationType.OIDC,
                                        ClientId = "client3.test.client",
                                        Uri = "https://security.test.company3.com/connect/token",
                                        ClientSecret = "verylongandsecuresecret3",
                                        Scopes = new [] { "activity1.webhook.api3.all" }
                                    }
                                },
                            }
                        }
                    }
                }
            };

            var result = await CallConverter(eventHandlerConfig);

            var expected = new[]
            {
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--1--uri' 'https://company1.com/api/' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--1--selector' 'selector1' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--1--authenticationconfig--type' 2 $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--1--authenticationconfig--clientid' 'client1.test.client' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--1--authenticationconfig--uri' 'https://security.test.company1.com/connect/token' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--1--authenticationconfig--clientsecret' 'verylongandsecuresecret1' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--1--authenticationconfig--scopes' 'activity1.webhook.api1.all' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--1--httpverb' 'POST' $KeyVault",

                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--2--uri' 'https://company2.com/api/' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--2--selector' 'selector2' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--2--authenticationconfig--type' 2 $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--2--authenticationconfig--clientid' 'client2.test.client' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--2--authenticationconfig--uri' 'https://security.test.company2.com/connect/token' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--2--authenticationconfig--clientsecret' 'verylongandsecuresecret2' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--2--authenticationconfig--scopes' 'activity1.webhook.api2.all' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--2--httpverb' 'POST' $KeyVault",

                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--3--uri' 'https://company3.com/api/' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--3--selector' 'selector3' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--3--authenticationconfig--type' 2 $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--3--authenticationconfig--clientid' 'client3.test.client' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--3--authenticationconfig--uri' 'https://security.test.company3.com/connect/token' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--3--authenticationconfig--clientsecret' 'verylongandsecuresecret3' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--3--authenticationconfig--scopes' 'activity1.webhook.api3.all' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--3--httpverb' 'POST' $KeyVault"
            };

            result.Should().Contain(expected);
        }

        [Fact, IsLayer0]
        public async Task ShouldBuildCallbackConfigName()
        {
            var eventHandlerConfig = new EventHandlerConfig
            {
                CallbackConfig = new WebhookConfig
                {
                    Name = "callback1-name",
                }
            };

            var result = await CallConverter(eventHandlerConfig);

            var expected = new[]
            {
                "setConfig 'event--1--callbackconfig--name' 'callback1-name' $KeyVault",
            };

            result.Should().Contain(expected);
        }

        [Fact, IsLayer0]
        public async Task ShouldBuildCallbackConfigOidcAuthenticationConfig()
        {
            var eventHandlerConfig = new EventHandlerConfig
            {
                CallbackConfig = new WebhookConfig
                {
                    AuthenticationConfig = new OidcAuthenticationConfig
                    {
                        Type = AuthenticationType.OIDC,
                        ClientId = "t.abc.client",
                        Uri = "https://security.site.com/connect/token",
                        ClientSecret = "verylongsecuresecret",
                        Scopes = new[]
                        {
                            "t.abc.client.api.all"
                        }
                    },
                }
            };

            var result = await CallConverter(eventHandlerConfig);

            var expected = new[]
            {
                "setConfig 'event--1--callbackconfig--authenticationconfig--type' 2 $KeyVault",
                "setConfig 'event--1--callbackconfig--authenticationconfig--uri' 'https://security.site.com/connect/token' $KeyVault",
                "setConfig 'event--1--callbackconfig--authenticationconfig--clientid' 't.abc.client' $KeyVault",
                "setConfig 'event--1--callbackconfig--authenticationconfig--clientsecret' 'verylongsecuresecret' $KeyVault",
                "setConfig 'event--1--callbackconfig--authenticationconfig--scopes' 't.abc.client.api.all' $KeyVault"
            };

            result.Should().Contain(expected);
        }

        [Fact, IsLayer0]
        public async Task ShouldBuildCallbackConfigWebhookRequestRule()
        {
            var eventHandlerConfig = new EventHandlerConfig
            {
                CallbackConfig = new WebhookConfig
                {
                    WebhookRequestRules = new List<WebhookRequestRule>
                    {
                        new WebhookRequestRule
                        {
                            Source = new ParserLocation
                            {
                                Path = "path1",
                                Type = DataType.Model
                            },
                            Destination = new ParserLocation
                            {
                                Type = DataType.Model
                            }
                        },
                        new WebhookRequestRule
                        {
                            Source = new ParserLocation
                            {
                                Path = "path2"
                            },
                            Destination = new ParserLocation
                            {
                                Location = Location.Uri,
                                RuleAction = RuleAction.Route
                            },
                        }
                    }
                }
            };

            var result = await CallConverter(eventHandlerConfig);

            var expected = new[]
            {
                "setConfig 'event--1--callbackconfig--webhookrequestrules--1--source--path' 'path1' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--1--source--type' 'Model' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--1--destination--type' 'Model' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--source--path' 'path2' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--destination--location' 'Uri' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--destination--ruleaction' 'route' $KeyVault"
            };

            result.Should().Contain(expected);
        }

        [Fact, IsLayer0]
        public async Task ShouldBuildCallbackConfigWebhookConfigRoutes()
        {
            var eventHandlerConfig = new EventHandlerConfig
            {
                CallbackConfig = new WebhookConfig
                {
                    WebhookRequestRules = new List<WebhookRequestRule>
                    {
                        new WebhookRequestRule
                        {
                        },
                        new WebhookRequestRule
                        {
                            Routes = new List<WebhookConfigRoute>
                            {
                                new WebhookConfigRoute
                                {
                                    Uri = "https://company1.com/api/",
                                    Selector = "selector1",
                                    HttpVerb = "POST",
                                    AuthenticationConfig = new OidcAuthenticationConfig
                                    {
                                        Type = AuthenticationType.OIDC,
                                        ClientId = "client1.test.client",
                                        Uri = "https://security.test.company1.com/connect/token",
                                        ClientSecret = "verylongandsecuresecret1",
                                        Scopes = new[] {"activity1.webhook.api1.all"}
                                    }
                                },
                                new WebhookConfigRoute
                                {
                                    Uri = "https://company2.com/api/",
                                    Selector = "selector2",
                                    HttpVerb = "POST",
                                    AuthenticationConfig = new OidcAuthenticationConfig
                                    {
                                        Type = AuthenticationType.OIDC,
                                        ClientId = "client2.test.client",
                                        Uri = "https://security.test.company2.com/connect/token",
                                        ClientSecret = "verylongandsecuresecret2",
                                        Scopes = new[] {"activity1.webhook.api2.all"}
                                    }
                                },
                                new WebhookConfigRoute
                                {
                                    Uri = "https://company3.com/api/",
                                    Selector = "selector3",
                                    HttpVerb = "POST",
                                    AuthenticationConfig = new OidcAuthenticationConfig
                                    {
                                        Type = AuthenticationType.OIDC,
                                        ClientId = "client3.test.client",
                                        Uri = "https://security.test.company3.com/connect/token",
                                        ClientSecret = "verylongandsecuresecret3",
                                        Scopes = new[] {"activity1.webhook.api3.all"}
                                    }
                                },
                            },
                        },
                    },
                }
            };

            var result = await CallConverter(eventHandlerConfig);

            var expected = new[]
            {
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--1--uri' 'https://company1.com/api/' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--1--selector' 'selector1' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--1--authenticationconfig--type' 2 $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--1--authenticationconfig--clientid' 'client1.test.client' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--1--authenticationconfig--uri' 'https://security.test.company1.com/connect/token' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--1--authenticationconfig--clientsecret' 'verylongandsecuresecret1' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--1--authenticationconfig--scopes' 'activity1.webhook.api1.all' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--1--httpverb' 'POST' $KeyVault",

                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--2--uri' 'https://company2.com/api/' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--2--selector' 'selector2' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--2--authenticationconfig--type' 2 $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--2--authenticationconfig--clientid' 'client2.test.client' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--2--authenticationconfig--uri' 'https://security.test.company2.com/connect/token' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--2--authenticationconfig--clientsecret' 'verylongandsecuresecret2' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--2--authenticationconfig--scopes' 'activity1.webhook.api2.all' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--2--httpverb' 'POST' $KeyVault",

                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--3--uri' 'https://company3.com/api/' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--3--selector' 'selector3' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--3--authenticationconfig--type' 2 $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--3--authenticationconfig--clientid' 'client3.test.client' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--3--authenticationconfig--uri' 'https://security.test.company3.com/connect/token' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--3--authenticationconfig--clientsecret' 'verylongandsecuresecret3' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--3--authenticationconfig--scopes' 'activity1.webhook.api3.all' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--3--httpverb' 'POST' $KeyVault"
            };

            result.Should().Contain(expected);
        }

        [Fact, IsLayer0]
        public async Task ShouldBuildWholeEventHandlerConfig()
        {
            var converter = new EventHandlerConfigToPowerShellConverter();

            var result = await converter.Convert(new[] { eventHandlerConfig });

            var missing = fullExpectedOutput.Except(result).ToList();

            string s = string.Join(Environment.NewLine, missing);

            result.Should().Contain(fullExpectedOutput);
        }

        [Fact, IsLayer0]
        public async Task ShouldBuildSubscriberConfigDetails()
        {
            var eventHandlerConfig = new EventHandlerConfig
            {
                Subscribers = new List<SubscriberConfiguration>
                {
                    new SubscriberConfiguration
                    {
                        Name = "subscriber1name",
                        EventType = "event1type",
                        SubscriberName = "subscriber1",
                        SourceSubscriptionName = "subscription1",
                        DLQMode = SubscriberDlqMode.WebHookMode,
                    }
                }
            };

            var result = await CallConverter(eventHandlerConfig);

            var expected = new[]
            {
                "setConfig 'event--1--subscribers--1--type' 'event1type' $KeyVault",
                "setConfig 'event--1--subscribers--1--name' 'subscriber1name' $KeyVault",
                "setConfig 'event--1--subscribers--1--subscribername' 'subscriber1' $KeyVault",
                "setConfig 'event--1--subscribers--1--SourceSubscriptionName' 'subscription1' $KeyVault",
                "setConfig 'event--1--subscribers--1--dlqmode' '1' $KeyVault",
            };

            result.Should().Contain(expected);
        }

        [Fact, IsLayer0]
        public async Task ShouldBuildSubscriberConfigWebhookRequestRules()
        {
            var eventHandlerConfig = new EventHandlerConfig
            {
                Subscribers = new List<SubscriberConfiguration>
                {
                    new SubscriberConfiguration
                    {
                        WebhookRequestRules = new List<WebhookRequestRule>
                        {
                            new WebhookRequestRule
                            {
                                Source = new ParserLocation
                                {
                                    Path = "path1",
                                },
                                Destination = new ParserLocation
                                {
                                    RuleAction = RuleAction.Route
                                }
                            },
                            new WebhookRequestRule
                            {
                                Source = new ParserLocation
                                {
                                    Path = "path2",
                                    Type = DataType.Model
                                },
                                Destination = new ParserLocation
                                {
                                    Type = DataType.Model
                                },
                            }
                        }
                    }
                }
            };

            var result = await CallConverter(eventHandlerConfig);

            var expected = new[]
            {
                "setConfig 'event--1--subscribers--1--webhookrequestrules--1--destination--ruleaction' 'route' $KeyVault",
                "setConfig 'event--1--subscribers--1--webhookrequestrules--1--source--path' 'path1' $KeyVault",
                "setConfig 'event--1--subscribers--1--webhookrequestrules--2--source--path' 'path2' $KeyVault",
                "setConfig 'event--1--subscribers--1--webhookrequestrules--2--source--type' 'Model' $KeyVault",
                "setConfig 'event--1--subscribers--1--webhookrequestrules--2--destination--type' 'Model' $KeyVault",
            };

            result.Should().Contain(expected);
        }

        [Fact, IsLayer0]
        public async Task ShouldBuildSubscriberConfigRequestRoutes()
        {
            var eventHandlerConfig = new EventHandlerConfig
            {
                Subscribers = new List<SubscriberConfiguration>
                {
                    new SubscriberConfiguration
                    {
                        WebhookRequestRules = new List<WebhookRequestRule>
                        {
                            new WebhookRequestRule
                            {
                                Routes = new List<WebhookConfigRoute>
                                {
                                    new WebhookConfigRoute
                                    {
                                        Uri = "https://route1.api.com/endpoint",
                                        Selector = "selector1",
                                        HttpVerb = "POST",
                                        AuthenticationConfig = new OidcAuthenticationConfig
                                        {
                                            Type = AuthenticationType.OIDC,
                                            ClientId = "client1.test.client",
                                            Uri = "https://security.test.company1.com/connect/token",
                                            ClientSecret = "verylongandsecuresecret1",
                                            Scopes = new[] {"activity1.webhook.api"}
                                        }
                                    },
                                    new WebhookConfigRoute
                                    {
                                        Uri = "https://route2.api.com/endpoint",
                                        Selector = "selector2",
                                        HttpVerb = "POST",
                                        AuthenticationConfig = new OidcAuthenticationConfig
                                        {
                                            Type = AuthenticationType.OIDC,
                                            ClientId = "client2.test.client",
                                            Uri = "https://security.test.company2.com/connect/token",
                                            ClientSecret = "verylongandsecuresecret2",
                                            Scopes = new[] {"activity2.webhook.api"}
                                        }
                                    },
                                },
                            },
                        }
                    }
                }
            };

            var result = await CallConverter(eventHandlerConfig);

            var expected = new[]
            {
                "setConfig 'event--1--subscribers--1--webhookrequestrules--1--routes--1--uri' 'https://route1.api.com/endpoint' $KeyVault",
                "setConfig 'event--1--subscribers--1--webhookrequestrules--1--routes--1--selector' 'selector1' $KeyVault",
                "setConfig 'event--1--subscribers--1--webhookrequestrules--1--routes--1--authenticationconfig--type' 2 $KeyVault",
                "setConfig 'event--1--subscribers--1--webhookrequestrules--1--routes--1--authenticationconfig--clientid' 'client1.test.client' $KeyVault",
                "setConfig 'event--1--subscribers--1--webhookrequestrules--1--routes--1--authenticationconfig--uri' 'https://security.test.company1.com/connect/token' $KeyVault",
                "setConfig 'event--1--subscribers--1--webhookrequestrules--1--routes--1--authenticationconfig--clientsecret' 'verylongandsecuresecret1' $KeyVault",
                "setConfig 'event--1--subscribers--1--webhookrequestrules--1--routes--1--authenticationconfig--scopes' 'activity1.webhook.api' $KeyVault",
                "setConfig 'event--1--subscribers--1--webhookrequestrules--1--routes--1--httpverb' 'POST' $KeyVault",

                "setConfig 'event--1--subscribers--1--webhookrequestrules--1--routes--2--uri' 'https://route2.api.com/endpoint' $KeyVault",
                "setConfig 'event--1--subscribers--1--webhookrequestrules--1--routes--2--selector' 'selector2' $KeyVault",
                "setConfig 'event--1--subscribers--1--webhookrequestrules--1--routes--2--authenticationconfig--type' 2 $KeyVault",
                "setConfig 'event--1--subscribers--1--webhookrequestrules--1--routes--2--authenticationconfig--clientid' 'client2.test.client' $KeyVault",
                "setConfig 'event--1--subscribers--1--webhookrequestrules--1--routes--2--authenticationconfig--uri' 'https://security.test.company2.com/connect/token' $KeyVault",
                "setConfig 'event--1--subscribers--1--webhookrequestrules--1--routes--2--authenticationconfig--clientsecret' 'verylongandsecuresecret2' $KeyVault",
                "setConfig 'event--1--subscribers--1--webhookrequestrules--1--routes--2--authenticationconfig--scopes' 'activity2.webhook.api' $KeyVault",
                "setConfig 'event--1--subscribers--1--webhookrequestrules--1--routes--2--httpverb' 'POST' $KeyVault",
            };

            result.Should().Contain(expected);
        }

        private static readonly EventHandlerConfig eventHandlerConfig = new EventHandlerConfig
        {
            Name = "activity1.domain.event1",
            Type = "activity1.domain.event1",
            WebhookConfig = new WebhookConfig
            {
                Name = "activity1.domain.event1-webhook",
                Uri = "https://site.com/api/v1/WebHook",
                AuthenticationConfig = new OidcAuthenticationConfig
                {
                    Type = AuthenticationType.OIDC,
                    ClientId = "t.abc.client",
                    Uri = "https://security.site.com/connect/token",
                    ClientSecret = "verylongsecuresecret",
                    Scopes = new[]
                    {
                        "t.abc.client.api.all"
                    }
                },
                WebhookRequestRules = new List<WebhookRequestRule>
                {
                    new WebhookRequestRule
                    {
                        Source = new ParserLocation
                        {
                            Path = "ActivityConfirmationRequestDto",
                            Type = DataType.Model
                        },
                        Destination = new ParserLocation
                        {
                            Type = DataType.Model
                        }
                    },
                    new WebhookRequestRule
                    {
                        Source = new ParserLocation
                        {
                            Path = "TenantCode"
                        },
                        Destination = new ParserLocation
                        {
                            RuleAction = RuleAction.Route
                        },
                        Routes = new List<WebhookConfigRoute>
                        {
                            new WebhookConfigRoute
                            {
                                Uri = "https://activity1-selector1-api.com/endpoint",
                                Selector = "selector1",
                                AuthenticationConfig = new OidcAuthenticationConfig
                                {
                                    Type = AuthenticationType.OIDC,
                                    ClientId = "clientid.test.client",
                                    Uri = "https://security.test.company.com/connect/token",
                                    ClientSecret = "verylongandsecuresecret",
                                    Scopes = new[]
                                    {
                                        "activity1.webhook.api.all"
                                    }
                                }
                            },
                            new WebhookConfigRoute
                            {
                                Uri = "https://activity1-selector2-api.com/endpoint",
                                Selector = "selector2",
                                AuthenticationConfig = new OidcAuthenticationConfig
                                {
                                    Type = AuthenticationType.OIDC,
                                    ClientId = "clientid.test.client",
                                    Uri = "https://security.test.company.com/connect/token",
                                    ClientSecret = "verylongandsecuresecret",
                                    Scopes = new[]
                                    {
                                        "activity1.webhook.api.all"
                                    }
                                }
                            },
                            new WebhookConfigRoute
                            {
                                Uri = "https://activity1-selector3-api.com/endpoint",
                                Selector = "selector3",
                                AuthenticationConfig = new OidcAuthenticationConfig
                                {
                                    Type = AuthenticationType.OIDC,
                                    ClientId = "clientid.test.client",
                                    Uri = "https://security.test.company.com/connect/token",
                                    ClientSecret = "verylongandsecuresecret",
                                    Scopes = new[]
                                    {
                                        "activity1.webhook.api.all"
                                    }
                                }
                            },
                        }
                    }
                }
            },
            CallbackConfig = new WebhookConfig
            {
                Name = "activity1.domain.event1-callback",
                WebhookRequestRules = new List<WebhookRequestRule>
                {
                    new WebhookRequestRule
                    {
                        Source = new ParserLocation
                        {
                            Path = "TenantCode"
                        },
                        Destination = new ParserLocation
                        {
                            RuleAction = RuleAction.Route
                        },
                        Routes = new List<WebhookConfigRoute>
                        {
                            new WebhookConfigRoute
                            {
                                Uri = "https://activity1-selector1-api.com/callback",
                                Selector = "selector1",
                                AuthenticationConfig = new OidcAuthenticationConfig
                                {
                                    Type = AuthenticationType.OIDC,
                                    ClientId = "clientid.test.client",
                                    Uri = "https://security.test.company.com/connect/token",
                                    ClientSecret = "verylongandsecuresecret",
                                    Scopes = new[]
                                    {
                                        "activity1.webhook.api.all"
                                    }
                                }
                            },
                            new WebhookConfigRoute
                            {
                                Uri = "https://activity1-selector2-api.com/callback",
                                Selector = "selector2",
                                AuthenticationConfig = new OidcAuthenticationConfig
                                {
                                    Type = AuthenticationType.OIDC,
                                    ClientId = "clientid.test.client",
                                    Uri = "https://security.test.company.com/connect/token",
                                    ClientSecret = "verylongandsecuresecret",
                                    Scopes = new[]
                                    {
                                        "activity1.webhook.api.all"
                                    }
                                }
                            },
                            new WebhookConfigRoute
                            {
                                Uri = "https://activity1-selector3-api.com/callback",
                                Selector = "selector3",
                                AuthenticationConfig = new OidcAuthenticationConfig
                                {
                                    Type = AuthenticationType.OIDC,
                                    ClientId = "clientid.test.client",
                                    Uri = "https://security.test.company.com/connect/token",
                                    ClientSecret = "verylongandsecuresecret",
                                    Scopes = new[]
                                    {
                                        "activity1.webhook.api.all"
                                    }
                                }
                            },
                        },
                    },
                    new WebhookRequestRule
                    {
                        Source = new ParserLocation
                        {
                            Type = DataType.HttpStatusCode
                        },
                        Destination = new ParserLocation
                        {
                            Path = "StatusCode"
                        }
                    },
                    new WebhookRequestRule
                    {
                        Source = new ParserLocation
                        {
                            Type = DataType.HttpContent
                        },
                        Destination = new ParserLocation
                        {
                            Path = "Content",
                            Type = DataType.String
                        }
                    },
                    new WebhookRequestRule
                    {
                        Source = new ParserLocation
                        {
                            Path = "OrderCode"
                        },
                        Destination = new ParserLocation
                        {
                            Location = Location.Uri
                        }
                    }
                }
            }
        };

        private readonly IEnumerable<string> fullExpectedOutput = new[]
        {
            "setConfig 'event--1--type' 'activity1.domain.event1' $KeyVault",
            "setConfig 'event--1--name' 'activity1.domain.event1' $KeyVault",
            "setConfig 'event--1--webhookconfig--name' 'activity1.domain.event1-webhook' $KeyVault",
            "setConfig 'event--1--webhookconfig--uri' 'https://site.com/api/v1/WebHook' $KeyVault",
            "setConfig 'event--1--webhookconfig--authenticationconfig--type' 2 $KeyVault",
            "setConfig 'event--1--webhookconfig--authenticationconfig--uri' 'https://security.site.com/connect/token' $KeyVault",
            "setConfig 'event--1--webhookconfig--authenticationconfig--clientid' 't.abc.client' $KeyVault",
            "setConfig 'event--1--webhookconfig--authenticationconfig--clientsecret' 'verylongsecuresecret' $KeyVault",
            "setConfig 'event--1--webhookconfig--authenticationconfig--scopes' 't.abc.client.api.all' $KeyVault",
            "setConfig 'event--1--webhookconfig--httpverb' 'POST' $KeyVault",

            "setConfig 'event--1--webhookconfig--webhookrequestrules--1--source--path' 'ActivityConfirmationRequestDto' $KeyVault",
            "setConfig 'event--1--webhookconfig--webhookrequestrules--1--source--type' 'Model' $KeyVault",
            "setConfig 'event--1--webhookconfig--webhookrequestrules--1--destination--type' 'Model' $KeyVault",
            "setConfig 'event--1--webhookconfig--webhookrequestrules--2--source--path' 'TenantCode' $KeyVault",
            "setConfig 'event--1--webhookconfig--webhookrequestrules--2--destination--ruleaction' 'route' $KeyVault",

            "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--1--uri' 'https://activity1-selector1-api.com/endpoint' $KeyVault",
            "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--1--selector' 'selector1' $KeyVault",
            "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--1--authenticationconfig--type' 2 $KeyVault",
            "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--1--authenticationconfig--clientid' 'clientid.test.client' $KeyVault",
            "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--1--authenticationconfig--uri' 'https://security.test.company.com/connect/token' $KeyVault",
            "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--1--authenticationconfig--clientsecret' 'verylongandsecuresecret' $KeyVault",
            "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--1--authenticationconfig--scopes' 'activity1.webhook.api.all' $KeyVault",
            "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--1--httpverb' 'POST' $KeyVault",

            "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--2--uri' 'https://activity1-selector2-api.com/endpoint' $KeyVault",
            "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--2--selector' 'selector2' $KeyVault",
            "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--2--authenticationconfig--type' 2 $KeyVault",
            "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--2--authenticationconfig--clientid' 'clientid.test.client' $KeyVault",
            "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--2--authenticationconfig--uri' 'https://security.test.company.com/connect/token' $KeyVault",
            "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--2--authenticationconfig--clientsecret' 'verylongandsecuresecret' $KeyVault",
            "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--2--authenticationconfig--scopes' 'activity1.webhook.api.all' $KeyVault",
            "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--2--httpverb' 'POST' $KeyVault",

            "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--3--uri' 'https://activity1-selector3-api.com/endpoint' $KeyVault",
            "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--3--selector' 'selector3' $KeyVault",
            "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--3--authenticationconfig--type' 2 $KeyVault",
            "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--3--authenticationconfig--clientid' 'clientid.test.client' $KeyVault",
            "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--3--authenticationconfig--uri' 'https://security.test.company.com/connect/token' $KeyVault",
            "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--3--authenticationconfig--clientsecret' 'verylongandsecuresecret' $KeyVault",
            "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--3--authenticationconfig--scopes' 'activity1.webhook.api.all' $KeyVault",
            "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--3--httpverb' 'POST' $KeyVault",

            "setConfig 'event--1--callbackconfig--name' 'activity1.domain.event1-callback' $KeyVault",

            "setConfig 'event--1--callbackconfig--webhookrequestrules--1--source--path' 'TenantCode' $KeyVault",
            "setConfig 'event--1--callbackconfig--webhookrequestrules--1--destination--ruleaction' 'route' $KeyVault",

            "setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--1--uri' 'https://activity1-selector1-api.com/callback' $KeyVault",
            "setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--1--selector' 'selector1' $KeyVault",
            "setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--1--authenticationconfig--type' 2 $KeyVault",
            "setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--1--authenticationconfig--clientid' 'clientid.test.client' $KeyVault",
            "setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--1--authenticationconfig--uri' 'https://security.test.company.com/connect/token' $KeyVault",
            "setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--1--authenticationconfig--clientsecret' 'verylongandsecuresecret' $KeyVault",
            "setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--1--authenticationconfig--scopes' 'activity1.webhook.api.all' $KeyVault",
            "setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--1--httpverb' 'POST' $KeyVault",

            "setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--2--uri' 'https://activity1-selector2-api.com/callback' $KeyVault",
            "setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--2--selector' 'selector2' $KeyVault",
            "setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--2--authenticationconfig--type' 2 $KeyVault",
            "setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--2--authenticationconfig--clientid' 'clientid.test.client' $KeyVault",
            "setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--2--authenticationconfig--uri' 'https://security.test.company.com/connect/token' $KeyVault",
            "setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--2--authenticationconfig--clientsecret' 'verylongandsecuresecret' $KeyVault",
            "setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--2--authenticationconfig--scopes' 'activity1.webhook.api.all' $KeyVault",
            "setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--2--httpverb' 'POST' $KeyVault",

            "setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--3--uri' 'https://activity1-selector3-api.com/callback' $KeyVault",
            "setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--3--selector' 'selector3' $KeyVault",
            "setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--3--authenticationconfig--type' 2 $KeyVault",
            "setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--3--authenticationconfig--clientid' 'clientid.test.client' $KeyVault",
            "setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--3--authenticationconfig--uri' 'https://security.test.company.com/connect/token' $KeyVault",
            "setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--3--authenticationconfig--clientsecret' 'verylongandsecuresecret' $KeyVault",
            "setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--3--authenticationconfig--scopes' 'activity1.webhook.api.all' $KeyVault",
            "setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--3--httpverb' 'POST' $KeyVault",

            "setConfig 'event--1--callbackconfig--webhookrequestrules--2--source--type' 'HttpStatusCode' $KeyVault",
            "setConfig 'event--1--callbackconfig--webhookrequestrules--2--destination--path' 'StatusCode' $KeyVault",
            "setConfig 'event--1--callbackconfig--webhookrequestrules--3--source--type' 'HttpContent' $KeyVault",
            "setConfig 'event--1--callbackconfig--webhookrequestrules--3--destination--path' 'Content' $KeyVault",
            "setConfig 'event--1--callbackconfig--webhookrequestrules--3--destination--type' 'String' $KeyVault",
            "setConfig 'event--1--callbackconfig--webhookrequestrules--4--source--path' 'OrderCode' $KeyVault",
            "setConfig 'event--1--callbackconfig--webhookrequestrules--4--destination--location' 'Uri' $KeyVault"
        };
    }
}
