using System.Collections.Generic;
using System.Linq;
using CaptainHook.Cli.Commands.GeneratePowerShell;
using CaptainHook.Cli.Extensions;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Xunit;

namespace CaptainHook.Cli.Tests.GeneratePowerShell
{
    public class EventHandlerConfigToPowerShellConverterTests
    {
        private static IEnumerable<string> CallConverter(params string[] contents)
        {
            var eventsData = new SortedDictionary<int, string>(contents.WithIndex().ToDictionary(k => k.index + 1, v => v.item));
            var result = new EventHandlerConfigToPowerShellConverter().Convert(eventsData);
            return result;
        }

        [Fact, IsUnit]
        public void ShouldBuildEventHandlerTypeAndName()
        {
            string eventHandlerConfig = @"{
                ""Name"": ""name1"",
                ""Type"": ""type1"",
            }";

            var result = CallConverter(eventHandlerConfig);

            var expected = new[]
            {
                "setConfig 'event--1--name' 'name1' $KeyVault",
                "setConfig 'event--1--type' 'type1' $KeyVault",
            };

            result.Should().Equal(expected);
        }

        [Fact, IsUnit]
        public void ShouldBuildEventHandlerTypeAndNameForMultipleEvents()
        {
            var eventHandlerConfig = @"{
                ""Name"":  ""name1"",
                ""Type"":  ""type1""
            }";

            var secondEventHandlerConfig = @"{
                ""Name"": ""name2"",
                ""Type"": ""type2""
            }";

            var result = CallConverter(eventHandlerConfig, secondEventHandlerConfig);

            var expected = new[]
            {
                "setConfig 'event--1--name' 'name1' $KeyVault",
                "setConfig 'event--1--type' 'type1' $KeyVault",
                "setConfig 'event--2--name' 'name2' $KeyVault",
                "setConfig 'event--2--type' 'type2' $KeyVault",
            };

            result.Should().Equal(expected);
        }

        [Fact, IsUnit]
        public void ShouldBuildWebHookConfigNameAndUrl()
        {
            var eventHandlerConfig = @"{
                ""WebhookConfig"": {
                    ""Name"": ""webhook1-name"",
                    ""Uri"": ""https://site.com/api/v1/WebHook1/"",
                    ""HttpVerb"": ""POST""
                    }
                 }";

            var result = CallConverter(eventHandlerConfig);

            var expected = new[]
            {
                "setConfig 'event--1--webhookconfig--name' 'webhook1-name' $KeyVault",
                "setConfig 'event--1--webhookconfig--uri' 'https://site.com/api/v1/WebHook1/' $KeyVault",
                "setConfig 'event--1--webhookconfig--httpverb' 'POST' $KeyVault"
            };

            result.Should().Equal(expected);
        }

        [Fact, IsUnit]
        public void ShouldBuildWebHookConfigOidcAuthenticationConfig()
        {
            var eventHandlerConfig = @"{
                ""WebhookConfig"": {
                    ""AuthenticationConfig"": {
                        ""Type"": ""OIDC"",
                        ""Uri"": ""https://security.site.com/connect/token"",
                        ""ClientId"": ""t.abc.client"",
                        ""ClientSecret"": ""verylongsecuresecret"",
                        ""Scopes"": [
                            ""t.abc.client.api.all""
                        ]
                    }
                }
            }";

            var result = CallConverter(eventHandlerConfig);

            var expected = new[]
            {
                "setConfig 'event--1--webhookconfig--authenticationconfig--type' 'OIDC' $KeyVault",
                "setConfig 'event--1--webhookconfig--authenticationconfig--uri' 'https://security.site.com/connect/token' $KeyVault",
                "setConfig 'event--1--webhookconfig--authenticationconfig--clientid' 't.abc.client' $KeyVault",
                "setConfig 'event--1--webhookconfig--authenticationconfig--clientsecret' 'verylongsecuresecret' $KeyVault",
                "setConfig 'event--1--webhookconfig--authenticationconfig--scopes' 't.abc.client.api.all' $KeyVault"
            };

            result.Should().Equal(expected);
        }

        [Fact, IsUnit]
        public void ShouldUnderstandNoneAuthenticationNodeAsNoneAuthenticationConfig()
        {
            var eventHandlerConfig = @"{
                ""WebhookConfig"": {
                    ""AuthenticationConfig"": { 
                        ""Type"": ""None""
                    }       
                }
            }";

            var result = CallConverter(eventHandlerConfig);

            var expected = new[]
            {
                "setConfig 'event--1--webhookconfig--authenticationconfig--type' 'None' $KeyVault",
            };

            result.Should().Equal(expected);
        }

        [Fact, IsUnit]
        public void ShouldBuildWebHookConfigWebhookRequestRule()
        {
            var eventHandlerConfig = @"{
                ""WebhookConfig"": {
                    ""WebhookRequestRules"": [{
                        ""Source"": {
                            ""Path"": ""path1"",
                            ""Type"": ""Model""
                        },
                        ""Destination"": {
                            ""Type"": ""Model""
                        }
                    }, {
                        ""Source"": {
                            ""Path"": ""path2""
                        },
                        ""Destination"": {
                            ""RuleAction"": ""Route""
                        }
                    }]
                }
            }";

            var result = CallConverter(eventHandlerConfig);

            var expected = new[]
            {
                "setConfig 'event--1--webhookconfig--webhookrequestrules--1--source--path' 'path1' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--1--source--type' 'Model' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--1--destination--type' 'Model' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--source--path' 'path2' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--destination--ruleaction' 'Route' $KeyVault"
            };

            result.Should().Equal(expected);
        }

        [Fact, IsUnit]
        public void ShouldBuildWebHookConfigWebhookConfigRoutes()
        {
            var eventHandlerConfig = @"{
                ""WebhookConfig"": {
                    ""WebhookRequestRules"": [{
                        }, {
                        ""Routes"": [{
                            ""Uri"": ""https://company1.com/api/"",
                            ""Selector"": ""selector1"",
                            ""AuthenticationConfig"": {
                                ""Type"": ""OIDC"",
                                ""ClientId"": ""client1.test.client"",
                                ""Uri"": ""https://security.test.company1.com/connect/token"",
                                ""ClientSecret"": ""verylongandsecuresecret1"",
                                ""Scopes"": [
                                    ""activity1.webhook.api1.all""
                                ]
                            },
                            ""HttpVerb"": ""POST""
                        }, {
                            ""Uri"": ""https://company2.com/api/"",
                            ""Selector"": ""selector2"",
                            ""AuthenticationConfig"": {
                                ""Type"": ""OIDC"",
                                ""ClientId"": ""client2.test.client"",
                                ""Uri"": ""https://security.test.company2.com/connect/token"",
                                ""ClientSecret"": ""verylongandsecuresecret2"",
                                ""Scopes"": [
                                    ""activity1.webhook.api2.all""
                                ]
                            },
                            ""HttpVerb"": ""POST""
                        }, {
                            ""Uri"": ""https://company3.com/api/"",
                            ""Selector"": ""selector3"",
                            ""AuthenticationConfig"": {
                                ""Type"": ""OIDC"",
                                ""ClientId"": ""client3.test.client"",
                                ""Uri"": ""https://security.test.company3.com/connect/token"",
                                ""ClientSecret"": ""verylongandsecuresecret3"",
                                ""Scopes"": [
                                    ""activity1.webhook.api3.all""
                                ]
                            },
                            ""HttpVerb"": ""POST""
                        }]
                    }]
                }
            }";

            var result = CallConverter(eventHandlerConfig);

            var expected = new[]
            {
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--1--uri' 'https://company1.com/api/' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--1--selector' 'selector1' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--1--authenticationconfig--type' 'OIDC' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--1--authenticationconfig--clientid' 'client1.test.client' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--1--authenticationconfig--uri' 'https://security.test.company1.com/connect/token' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--1--authenticationconfig--clientsecret' 'verylongandsecuresecret1' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--1--authenticationconfig--scopes' 'activity1.webhook.api1.all' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--1--httpverb' 'POST' $KeyVault",

                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--2--uri' 'https://company2.com/api/' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--2--selector' 'selector2' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--2--authenticationconfig--type' 'OIDC' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--2--authenticationconfig--clientid' 'client2.test.client' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--2--authenticationconfig--uri' 'https://security.test.company2.com/connect/token' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--2--authenticationconfig--clientsecret' 'verylongandsecuresecret2' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--2--authenticationconfig--scopes' 'activity1.webhook.api2.all' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--2--httpverb' 'POST' $KeyVault",

                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--3--uri' 'https://company3.com/api/' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--3--selector' 'selector3' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--3--authenticationconfig--type' 'OIDC' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--3--authenticationconfig--clientid' 'client3.test.client' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--3--authenticationconfig--uri' 'https://security.test.company3.com/connect/token' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--3--authenticationconfig--clientsecret' 'verylongandsecuresecret3' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--3--authenticationconfig--scopes' 'activity1.webhook.api3.all' $KeyVault",
                "setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--3--httpverb' 'POST' $KeyVault"
            };

            result.Should().Equal(expected);
        }

        [Fact, IsUnit]
        public void ShouldBuildCallbackConfigName()
        {
            var eventHandlerConfig = @"{
                ""CallbackConfig"": {
                    ""Name"": ""callback1-name""
                    }
                 }";

            var result = CallConverter(eventHandlerConfig);

            var expected = new[]
            {
                "setConfig 'event--1--callbackconfig--name' 'callback1-name' $KeyVault",
            };

            result.Should().Equal(expected);
        }

        [Fact, IsUnit]
        public void ShouldBuildCallbackConfigOidcAuthenticationConfig()
        {
            var eventHandlerConfig = @"{
                ""CallbackConfig"": {
                    ""AuthenticationConfig"": {
                        ""Type"": ""OIDC"",                       
                        ""Uri"": ""https://security.site.com/connect/token"",
                        ""ClientId"": ""t.abc.client"",
                        ""ClientSecret"": ""verylongsecuresecret"",
                        ""Scopes"": [
                            ""t.abc.client.api.all""
                        ]
                    }
                }
            }";

            var result = CallConverter(eventHandlerConfig);

            var expected = new[]
            {
                "setConfig 'event--1--callbackconfig--authenticationconfig--type' 'OIDC' $KeyVault",
                "setConfig 'event--1--callbackconfig--authenticationconfig--uri' 'https://security.site.com/connect/token' $KeyVault",
                "setConfig 'event--1--callbackconfig--authenticationconfig--clientid' 't.abc.client' $KeyVault",
                "setConfig 'event--1--callbackconfig--authenticationconfig--clientsecret' 'verylongsecuresecret' $KeyVault",
                "setConfig 'event--1--callbackconfig--authenticationconfig--scopes' 't.abc.client.api.all' $KeyVault"
            };

            result.Should().Equal(expected);
        }

        [Fact, IsUnit]
        public void ShouldBuildCallbackConfigWebhookRequestRule()
        {
            var eventHandlerConfig = @"{
                ""CallbackConfig"": {
                    ""WebhookRequestRules"": [{
                        ""Source"": {
                            ""Path"": ""path1"",
                            ""Type"": ""Model""
                        },
                        ""Destination"": {
                            ""Type"": ""Model""
                        }
                    }, {
                        ""Source"": {
                            ""Path"": ""path2""
                        },
                        ""Destination"": {
                            ""Location"": ""Uri"",
                            ""RuleAction"": ""Route""
                        }
                    }]
                }
            }";

            var result = CallConverter(eventHandlerConfig);

            var expected = new[]
            {
                "setConfig 'event--1--callbackconfig--webhookrequestrules--1--source--path' 'path1' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--1--source--type' 'Model' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--1--destination--type' 'Model' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--source--path' 'path2' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--destination--location' 'Uri' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--destination--ruleaction' 'Route' $KeyVault"
            };

            result.Should().Equal(expected);
        }

        [Fact, IsUnit]
        public void ShouldIgnoreEmptyDestination()
        {
            var eventHandlerConfig = @"{
                ""CallbackConfig"": {
                    ""WebhookRequestRules"": [{
                        ""Source"": {
                            ""Path"": ""path1"",
                            ""Type"": ""Model""
                        },
                        ""Destination"": {
                        }
                    }]
                }
            }";

            var result = CallConverter(eventHandlerConfig);

            var expected = new[]
            {
                "setConfig 'event--1--callbackconfig--webhookrequestrules--1--source--path' 'path1' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--1--source--type' 'Model' $KeyVault",
            };

            result.Should().Equal(expected);
        }

        [Fact, IsUnit]
        public void ShouldBuildCallbackConfigWebhookConfigRoutes()
        {
            var eventHandlerConfig = @"{
                ""CallbackConfig"": {
                    ""WebhookRequestRules"": [{
                        }, {
                        ""Routes"": [{
                            ""Uri"": ""https://company1.com/api/"",
                            ""Selector"": ""selector1"",
                            ""AuthenticationConfig"": {
                                ""Type"": ""OIDC"",
                                ""ClientId"": ""client1.test.client"",
                                ""Uri"": ""https://security.test.company1.com/connect/token"",
                                ""ClientSecret"": ""verylongandsecuresecret1"",
                                ""Scopes"": [
                                    ""activity1.webhook.api1.all""
                                ]
                            },
                            ""HttpVerb"": ""POST""
                        }, {
                            ""Uri"": ""https://company2.com/api/"",
                            ""Selector"": ""selector2"",
                            ""AuthenticationConfig"": {
                                ""Type"": ""OIDC"",
                                ""ClientId"": ""client2.test.client"",
                                ""Uri"": ""https://security.test.company2.com/connect/token"",
                                ""ClientSecret"": ""verylongandsecuresecret2"",
                                ""Scopes"": [
                                    ""activity1.webhook.api2.all""
                                ]
                            },
                            ""HttpVerb"": ""POST""
                        }, {
                            ""Uri"": ""https://company3.com/api/"",
                            ""Selector"": ""selector3"",
                            ""AuthenticationConfig"": {
                                ""Type"": ""OIDC"",
                                ""ClientId"": ""client3.test.client"",
                                ""Uri"": ""https://security.test.company3.com/connect/token"",
                                ""ClientSecret"": ""verylongandsecuresecret3"",
                                ""Scopes"": [
                                    ""activity1.webhook.api3.all""
                                ]
                            },
                            ""HttpVerb"": ""POST""
                        }]
                    }]
                }
            }";

            var result = CallConverter(eventHandlerConfig);

            var expected = new[]
            {
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--1--uri' 'https://company1.com/api/' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--1--selector' 'selector1' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--1--authenticationconfig--type' 'OIDC' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--1--authenticationconfig--clientid' 'client1.test.client' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--1--authenticationconfig--uri' 'https://security.test.company1.com/connect/token' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--1--authenticationconfig--clientsecret' 'verylongandsecuresecret1' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--1--authenticationconfig--scopes' 'activity1.webhook.api1.all' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--1--httpverb' 'POST' $KeyVault",

                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--2--uri' 'https://company2.com/api/' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--2--selector' 'selector2' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--2--authenticationconfig--type' 'OIDC' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--2--authenticationconfig--clientid' 'client2.test.client' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--2--authenticationconfig--uri' 'https://security.test.company2.com/connect/token' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--2--authenticationconfig--clientsecret' 'verylongandsecuresecret2' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--2--authenticationconfig--scopes' 'activity1.webhook.api2.all' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--2--httpverb' 'POST' $KeyVault",

                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--3--uri' 'https://company3.com/api/' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--3--selector' 'selector3' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--3--authenticationconfig--type' 'OIDC' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--3--authenticationconfig--clientid' 'client3.test.client' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--3--authenticationconfig--uri' 'https://security.test.company3.com/connect/token' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--3--authenticationconfig--clientsecret' 'verylongandsecuresecret3' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--3--authenticationconfig--scopes' 'activity1.webhook.api3.all' $KeyVault",
                "setConfig 'event--1--callbackconfig--webhookrequestrules--2--routes--3--httpverb' 'POST' $KeyVault"
            };

            result.Should().Equal(expected);
        }

        [Fact, IsUnit]
        public void ShouldBuildSubscriberConfigDetails()
        {
            var eventHandlerConfig = @"{
                    ""Subscribers"": [{
                        ""Type"": ""event1type"",
                        ""Name"": ""subscriber1name"",
                        ""SubscriberName"": ""subscriber1"",
                        ""SourceSubscriptionName"": ""subscription1"",
                        ""DLQMode"": ""WebHookMode"",
                    }]
                }";

            var result = CallConverter(eventHandlerConfig);

            var expected = new[]
            {
                "setConfig 'event--1--subscribers--1--type' 'event1type' $KeyVault",
                "setConfig 'event--1--subscribers--1--name' 'subscriber1name' $KeyVault",
                "setConfig 'event--1--subscribers--1--subscribername' 'subscriber1' $KeyVault",
                "setConfig 'event--1--subscribers--1--sourcesubscriptionname' 'subscription1' $KeyVault",
                "setConfig 'event--1--subscribers--1--dlqmode' 'WebHookMode' $KeyVault",
            };

            result.Should().Equal(expected);
        }

        [Fact, IsUnit]
        public void ShouldBuildSubscriberConfigWebhookRequestRules()
        {
            var eventHandlerConfig = @"{
                ""Subscribers"": [{
                    ""WebhookRequestRules"": [{
                        ""Source"": {
                            ""Path"": ""path1"",
                        },
                        ""Destination"": {
                            ""RuleAction"": ""Route""
                        }
                    }, {
                        ""Source"": {
                            ""Path"": ""path2"",
                            ""Type"": ""Model""
                        },
                        ""Destination"": {
                            ""Type"": ""Model""
                        }
                    }]
                }]
            }";

            var result = CallConverter(eventHandlerConfig);

            var expected = new[]
            {
                "setConfig 'event--1--subscribers--1--webhookrequestrules--1--source--path' 'path1' $KeyVault",
                "setConfig 'event--1--subscribers--1--webhookrequestrules--1--destination--ruleaction' 'Route' $KeyVault",
                "setConfig 'event--1--subscribers--1--webhookrequestrules--2--source--path' 'path2' $KeyVault",
                "setConfig 'event--1--subscribers--1--webhookrequestrules--2--source--type' 'Model' $KeyVault",
                "setConfig 'event--1--subscribers--1--webhookrequestrules--2--destination--type' 'Model' $KeyVault",
            };

            result.Should().Equal(expected);
        }

        [Fact, IsUnit]
        public void ShouldBuildSubscriberConfigRequestRoutes()
        {
            var eventHandlerConfig = @"{
                ""Subscribers"": [{
                    ""WebhookRequestRules"": [{
                    }, {
                       ""Routes"": [{
                            ""Uri"": ""https://company1.com/api/"",
                            ""Selector"": ""selector1"",
                            ""AuthenticationConfig"": {
                                ""Type"": ""OIDC"",
                                ""ClientId"": ""client1.test.client"",
                                ""Uri"": ""https://security.test.company1.com/connect/token"",
                                ""ClientSecret"": ""verylongandsecuresecret1"",
                                ""Scopes"": [
                                    ""activity1.webhook.api1.all""
                                ]
                            },
                            ""HttpVerb"": ""POST""
                        }, {
                            ""Uri"": ""https://company2.com/api/"",
                            ""Selector"": ""selector2"",
                            ""AuthenticationConfig"": {
                                ""Type"": ""OIDC"",
                                ""ClientId"": ""client2.test.client"",
                                ""Uri"": ""https://security.test.company2.com/connect/token"",
                                ""ClientSecret"": ""verylongandsecuresecret2"",
                                ""Scopes"": [
                                    ""activity1.webhook.api2.all""
                                ]
                            },  
                            ""HttpVerb"": ""POST"",
                        }]
                    }]
                }]
            }";

            var result = CallConverter(eventHandlerConfig);

            var expected = new[]
            {
                "setConfig 'event--1--subscribers--1--webhookrequestrules--2--routes--1--uri' 'https://company1.com/api/' $KeyVault",
                "setConfig 'event--1--subscribers--1--webhookrequestrules--2--routes--1--selector' 'selector1' $KeyVault",
                "setConfig 'event--1--subscribers--1--webhookrequestrules--2--routes--1--authenticationconfig--type' 'OIDC' $KeyVault",
                "setConfig 'event--1--subscribers--1--webhookrequestrules--2--routes--1--authenticationconfig--clientid' 'client1.test.client' $KeyVault",
                "setConfig 'event--1--subscribers--1--webhookrequestrules--2--routes--1--authenticationconfig--uri' 'https://security.test.company1.com/connect/token' $KeyVault",
                "setConfig 'event--1--subscribers--1--webhookrequestrules--2--routes--1--authenticationconfig--clientsecret' 'verylongandsecuresecret1' $KeyVault",
                "setConfig 'event--1--subscribers--1--webhookrequestrules--2--routes--1--authenticationconfig--scopes' 'activity1.webhook.api1.all' $KeyVault",
                "setConfig 'event--1--subscribers--1--webhookrequestrules--2--routes--1--httpverb' 'POST' $KeyVault",

                "setConfig 'event--1--subscribers--1--webhookrequestrules--2--routes--2--uri' 'https://company2.com/api/' $KeyVault",
                "setConfig 'event--1--subscribers--1--webhookrequestrules--2--routes--2--selector' 'selector2' $KeyVault",
                "setConfig 'event--1--subscribers--1--webhookrequestrules--2--routes--2--authenticationconfig--type' 'OIDC' $KeyVault",
                "setConfig 'event--1--subscribers--1--webhookrequestrules--2--routes--2--authenticationconfig--clientid' 'client2.test.client' $KeyVault",
                "setConfig 'event--1--subscribers--1--webhookrequestrules--2--routes--2--authenticationconfig--uri' 'https://security.test.company2.com/connect/token' $KeyVault",
                "setConfig 'event--1--subscribers--1--webhookrequestrules--2--routes--2--authenticationconfig--clientsecret' 'verylongandsecuresecret2' $KeyVault",
                "setConfig 'event--1--subscribers--1--webhookrequestrules--2--routes--2--authenticationconfig--scopes' 'activity1.webhook.api2.all' $KeyVault",
                "setConfig 'event--1--subscribers--1--webhookrequestrules--2--routes--2--httpverb' 'POST' $KeyVault",
            };

            result.Should().Equal(expected);
        }
    }
}