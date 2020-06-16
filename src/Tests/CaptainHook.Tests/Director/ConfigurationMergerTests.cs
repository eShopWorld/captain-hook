using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.DirectorService.Infrastructure;
using CaptainHook.Domain.Models;
using CaptainHook.Tests.Builders;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Xunit;

namespace CaptainHook.Tests.Director
{
    public class ConfigurationMergerTests
    {
        private readonly SubscriberConfiguration[] _kvSubscribers =
        {
            new SubscriberConfigurationBuilder().WithType("testevent").WithCallback().Create(),
            new SubscriberConfigurationBuilder().WithType("testevent").WithSubscriberName("subscriber1").Create(),
            new SubscriberConfigurationBuilder().WithType("testevent.completed").Create(),
        };

        [Fact, IsLayer0]
        public void OnlyKVSubscribers_AllInResult()
        {
            var result = new ConfigurationMerger().Merge(_kvSubscribers, new List<Subscriber>());

            result.Should().HaveCount(3);
            result.Should().Contain(x => x.Name == "testevent" && x.SubscriberName == "captain-hook");
            result.Should().Contain(x => x.Name == "testevent" && x.SubscriberName == "subscriber1");
            result.Should().Contain(x => x.Name == "testevent.completed" && x.SubscriberName == "captain-hook");
        }

        [Fact, IsLayer0]
        public void OnlyCosmosSubscribers_AllInResult()
        {
            var cosmosSubscribers = new Subscriber[]
            {
                new Subscriber
                {
                    Name = "captain-hook",
                    Webhooks = new Webhooks
                    {
                        Endpoints =
                        {
                            new Endpoint
                            {
                                Uri = "https://cosmos.eshopworld.com/testevent/",
                                HttpVerb = "POST",
                                Authentication = null,
                            },
                        },
                    },
                    Event = new Event
                    {
                        Name = "testevent",
                        Type = "testevent",
                    }
                },
                new Subscriber
                {
                    Name = "captain-hook",
                    Webhooks = new Webhooks
                    {
                        Endpoints =
                        {
                            new Endpoint
                            {
                                Uri = "https://cosmos.eshopworld.com/testevent-completed/",
                                HttpVerb = "POST",
                                Authentication = null,
                            },
                        },
                    },
                    Event = new Event
                    {
                        Name = "testevent.completed",
                        Type = "testevent.completed",
                    }
                },
                new Subscriber
                {
                    Name = "subscriber1",
                    Webhooks = new Webhooks
                    {
                        Endpoints =
                        {
                            new Endpoint
                            {
                                Uri = "https://cosmos.eshopworld.com/testevent2/",
                                HttpVerb = "POST",
                                Authentication = null,
                            },
                        },
                    },
                    Event = new Event
                    {
                        Name = "testevent",
                        Type = "testevent",
                    }
                },
            };

            var result = new ConfigurationMerger().Merge(new List<SubscriberConfiguration>(), cosmosSubscribers);

            result.Should().HaveCount(3);
            result.Should().Contain(x => x.Name == "testevent" && x.SubscriberName == "captain-hook");
            result.Should().Contain(x => x.Name == "testevent" && x.SubscriberName == "subscriber1");
            result.Should().Contain(x => x.Name == "testevent.completed" && x.SubscriberName == "captain-hook");
        }

        [Fact, IsLayer0]
        public void WhenSameEventsExistInKvSubscribersAndCosomosSubscribers_CosmosSubscribersMustOverrideKvSubscribers()
        {
            var cosmosSubscribers = new Subscriber[]
            {
                new Subscriber
                {
                    Name = "captain-hook",
                    Webhooks = new Webhooks
                    {
                        Endpoints =
                        {
                            new Endpoint
                            {
                                Uri = "https://cosmos.eshopworld.com/testevent/",
                                HttpVerb = "POST",
                                Authentication = null,
                            },
                        },
                    },
                    Event = new Event
                    {
                        Name = "testevent",
                        Type = "testevent",
                    }
                },
                new Subscriber
                {
                    Name = "captain-hook",
                    Webhooks = new Webhooks
                    {
                        Endpoints =
                        {
                            new Endpoint
                            {
                                Uri = "https://cosmos.eshopworld.com/newtestevent-completed/",
                                HttpVerb = "POST",
                                Authentication = null,
                            },
                        },
                    },
                    Event = new Event
                    {
                        Name = "newtestevent.completed",
                        Type = "newtestevent.completed",
                    }
                },
                new Subscriber
                {
                    Name = "subscriber1",
                    Webhooks = new Webhooks
                    {
                        Endpoints =
                        {
                            new Endpoint
                            {
                                Uri = "https://cosmos.eshopworld.com/newtestevent2/",
                                HttpVerb = "POST",
                                Authentication = null,
                            },
                        },
                    },
                    Event = new Event
                    {
                        Name = "newtestevent",
                        Type = "newtestevent",
                    }
                },
            };

            var result = new ConfigurationMerger().Merge(_kvSubscribers, cosmosSubscribers);

            result.Should().HaveCount(5);

            result.Should().Contain(x => x.Name == "testevent" && x.SubscriberName == "captain-hook" && x.Uri == "https://cosmos.eshopworld.com/testevent/");

            result.Should().Contain(x => x.Name == "testevent" && x.SubscriberName == "subscriber1" && x.Uri == "https://blah.blah.eshopworld.com/webhook/");
            result.Should().Contain(x => x.Name == "testevent.completed" && x.SubscriberName == "captain-hook" && x.Uri == "https://blah.blah.eshopworld.com/webhook/");

            result.Should().Contain(x => x.Name == "newtestevent" && x.SubscriberName == "subscriber1" && x.Uri == "https://cosmos.eshopworld.com/testevent2/");
            result.Should().Contain(x => x.Name == "newtestevent.completed" && x.SubscriberName == "captain-hook" && x.Uri == "https://cosmos.eshopworld.com/newtestevent-completed/");
        }

        [Fact, IsLayer0]
        public void CosmosSubscriberWithAuthentication_ShouldBeMappedProperly()
        {
            var cosmosSubscribers = new Subscriber[]
            {
                new Subscriber
                {
                    Name = "captain-hook",
                    Webhooks = new Webhooks
                    {
                        Endpoints =
                        {
                            new Endpoint
                            {
                                Uri = "https://cosmos.eshopworld.com/testevent/",
                                HttpVerb = "POST",
                                Authentication = new Authentication()
                                {
                                    Uri = "https://blah-blah.sts.eshopworld.com",
                                    ClientId = "captain-hook-id",
                                    Secret = "verylongpassword",
                                },
                            },
                        },
                    },
                    Event = new Event
                    {
                        Name = "testevent",
                        Type = "testevent",
                    }
                },
            };

            var result = new ConfigurationMerger().Merge(new List<SubscriberConfiguration>(), cosmosSubscribers);

            result.Should().HaveCount(3);
            result.Should().Contain(x => x.Name == "testevent" && x.SubscriberName == "captain-hook" && x.AuthenticationConfig is OidcAuthenticationConfig);
        }

        [Fact, IsLayer0]
        public void CosmosSubscriberWithCallback_ShouldBeMappedProperly()
        {
            var cosmosSubscribers = new Subscriber[]
            {
                new Subscriber
                {
                    Name = "captain-hook",
                    Webhooks = new Webhooks
                    {
                        Endpoints =
                        {
                            new Endpoint
                            {
                                Uri = "https://cosmos.eshopworld.com/testevent/",
                                HttpVerb = "POST",
                                Authentication = null,
                            },
                        },
                    },
                    Callbacks = new Webhooks
                    {
                        Endpoints =
                        {
                            new Endpoint
                            {
                                Uri = "https://cosmos.eshopworld.com/callback/",
                                HttpVerb = "PUT",
                                Authentication = null,
                            },
                        },
                    },
                    Event = new Event
                    {
                        Name = "testevent",
                        Type = "testevent",
                    }
                },
            };

            var result = new ConfigurationMerger().Merge(new List<SubscriberConfiguration>(), cosmosSubscribers);

            result.Should().HaveCount(3);
            result.Should().Contain(x => x.Name == "testevent" 
                                         && x.SubscriberName == "captain-hook" && x.Callback.Uri == "https://cosmos.eshopworld.com/callback/" && x.Callback.HttpVerb == "PUT");
        }

        [Fact, IsLayer0]
        public void CosmosSubscriberWithDlq_ShouldBeMappedProperly()
        {
            var cosmosSubscribers = new Subscriber[]
            {
                new Subscriber
                {
                    Name = "captain-hook",
                    Webhooks = new Webhooks
                    {
                        Endpoints =
                        {
                            new Endpoint
                            {
                                Uri = "https://cosmos.eshopworld.com/testevent/",
                                HttpVerb = "POST",
                                Authentication = null,
                            },
                        },
                    },
                    Dlq = new Webhooks
                    {
                        Endpoints =
                        {
                            new Endpoint
                            {
                                Uri = "https://cosmos.eshopworld.com/dlq/",
                                HttpVerb = "PUT",
                                Authentication = null,
                            },
                        },
                    },
                    Event = new Event
                    {
                        Name = "testevent",
                        Type = "testevent",
                    }
                },
            };

            var result = new ConfigurationMerger().Merge(new List<SubscriberConfiguration>(), cosmosSubscribers);

            result.Should().HaveCount(3);
            result.Should().Contain(x => x.Name == "testevent-dlq" && x.SubscriberName == "DLQ"
                                                                   && x.WebhookRequestRules.SelectMany(w => w.Routes).All(r => r.Uri == "https://cosmos.eshopworld.com/dlq/"));
        }

        internal class SubscriberBuilder
        {
            private string _name = "event1";

            private string subscriberName = "captain-hook";
            private string uri = "https://blah.blah.eshopworld.com";
            private List<WebhookRequestRule> webhookRequestRules;
            private WebhookConfig callback;

            private Event _event;
            private Webhooks _webhooks;
            private Webhooks _callbacks;

            public SubscriberBuilder WithName(string name)
            {
                _name = name;
                return this;
            }

            public SubscriberBuilder WithSubscriberName(string subscriberName)
            {
                this.subscriberName = subscriberName;
                return this;
            }

            public SubscriberBuilder WithUri(string uri)
            {
                this.uri = uri;
                return this;
            }

            //public SubscriberBuilder WithOidcAuthentication()
            //{
            //    this.authenticationConfig = new OidcAuthenticationConfig
            //    {
            //        Type = AuthenticationType.OIDC,
            //        Uri = "https://blah-blah.sts.eshopworld.com",
            //        ClientId = "ClientId",
            //        ClientSecret = "ClientSecret",
            //        Scopes = new[] { "scope1", "scope2" }
            //    };

            //    return this;
            //}

            // public SubscriberBuilder WithCallback(string uri = "https://callback.eshopworld.com")
            // {
            //     this.callback = new WebhookConfig
            //     {
            //         Name = "callback",
            //         HttpMethod = HttpMethod.Post,
            //         Uri = uri,
            //         EventType = "event1",
            //         AuthenticationConfig = new AuthenticationConfig
            //         {
            //             Type = AuthenticationType.None
            //         },
            //     };
            //
            //     return this;
            // }

            // public SubscriberBuilder AddWebhookRequestRule(Action<WebhookRequestRuleBuilder> ruleBuilder)
            // {
            //     if (this.webhookRequestRules == null)
            //     {
            //         this.webhookRequestRules = new List<WebhookRequestRule>();
            //     }
            //
            //     var rb = new WebhookRequestRuleBuilder();
            //     ruleBuilder(rb);
            //     this.webhookRequestRules.Add(rb.Create());
            //     return this;
            // }

            public Subscriber Create()
            {
                var subscriber = new Subscriber
                {
                    Name = _name,
                    Event = _event,
                    Webhooks = _webhooks,
                    Callbacks = _callbacks,
                };

                return subscriber;
            }
        }
    }
}