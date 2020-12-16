using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using CaptainHook.Common;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.EventHandlerActor.Handlers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CaptainHook.Benchmark
{
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [RPlotExporter, RankColumn]
    public class RequestBuilderBenchmark
    {
        private WebhookConfig _config;
        private string _data;

        public static void Main()
        {
            _ = BenchmarkRunner.Run<RequestBuilderBenchmark>();
        }

        [GlobalSetup]
        public void Setup()
        {
            _config = new WebhookConfig
            {
                Name = "Webhook2",
                HttpMethod = HttpMethod.Post,
                Uri = "https://blah.blah.eshopworld.com/webhook/",
                WebhookRequestRules = new List<WebhookRequestRule>
                {
                    new WebhookRequestRule
                    {
                        Source = new SourceParserLocation {Path = "OrderCode"},
                        Destination = new ParserLocation {Location = Location.Uri}
                    },
                    new WebhookRequestRule
                    {
                        Source = new SourceParserLocation {Path = "BrandType"},
                        Destination = new ParserLocation
                        {
                            RuleAction = RuleAction.Route
                        },
                        Routes = new List<WebhookConfigRoute>
                        {
                            new WebhookConfigRoute
                            {
                                Uri = "https://blah.blah.brand1.eshopworld.com/webhook",
                                HttpMethod = HttpMethod.Post,
                                Selector = "Brand1",
                                AuthenticationConfig = new AuthenticationConfig
                                {
                                    Type = AuthenticationType.None
                                }
                            },
                            new WebhookConfigRoute
                            {
                                Uri = "https://blah.blah.brand2.eshopworld.com/webhook",
                                HttpMethod = HttpMethod.Post,
                                Selector = "Brand2",
                                AuthenticationConfig = new AuthenticationConfig
                                {
                                    Type = AuthenticationType.None
                                }
                            }
                        }
                    },
                    new WebhookRequestRule {Source = new SourceParserLocation {Path = "OrderConfirmationRequestDto"}}
                }
            };

            _data = "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand1\"}";
        }

        [Benchmark]
        public void BenchmarkBuildUriV1()
        {
            BuildUriV1(_config, _data);
        }

        [Benchmark]
        public void BenchmarkBuildUriV2()
        {
            //var builder = new RequestBuilder(new BigBrother("", "")); //this requires AI key to be populated by dev before running a benchmark
            //builder.BuildUri(_config, _data);
        }

        public string BuildUriV1(WebhookConfig config, string payload)
        {
            var uri = config.Uri;
            //build the uri from the routes first
            var routingRules = config.WebhookRequestRules.FirstOrDefault(l => l.Routes.Any());
            if (routingRules != null)
            {
                if (routingRules.Source.Location == Location.Body)
                {
                    var path = routingRules.Source.Path;
                    var value = ModelParser.ParsePayloadPropertyAsString(path, payload);

                    if (string.IsNullOrWhiteSpace(value))
                    {
                        throw new ArgumentNullException(nameof(path), "routing path value in message payload is null or empty");
                    }


                    //selects the route based on the value found in the payload of the message
                    var rules = config.WebhookRequestRules.FirstOrDefault(r => r.Routes.Any());
                    var route = rules?.Routes.FirstOrDefault(r => r.Selector.Equals(value, StringComparison.OrdinalIgnoreCase));
                    if (route == null)
                    {
                        throw new Exception("route mapping/selector not found between config and the properties on the domain object");
                    }
                    uri = route.Uri;
                }
            }

            //after route has been selected then select the identifier for the RESTful URI if applicable
            var uriRules = config.WebhookRequestRules.FirstOrDefault(l => l.Destination.Location == Location.Uri);
            if (uriRules == null)
            {
                return uri;
            }

            if (uriRules.Source.Location != Location.Body)
            {
                return uri;
            }

            var parameter = ModelParser.ParsePayloadPropertyAsString(uriRules.Source.Path, payload);
            uri = CombineUriAndResourceId(uri, parameter);
            return uri;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private static string CombineUriAndResourceId(string uri, string parameter)
        {
            var position = uri.LastIndexOfSafe('/');
            uri = position == uri.Length - 1 ? $"{uri}{parameter}" : $"{uri}/{parameter}";
            return uri;
        }

        public string BuildPayload(WebhookConfig config, string sourcePayload, IDictionary<string, object> metadata = null)
        {
            var rules = config.WebhookRequestRules.Where(l => l.Destination.Location == Location.Body).ToList();

            if (!rules.Any())
            {
                return sourcePayload;
            }

            //Any replace action replaces the payload 
            var replaceRule = rules.FirstOrDefault(r => r.Destination.RuleAction == RuleAction.Replace);
            if (replaceRule != null)
            {
                var destinationPayload = ModelParser.ParsePayloadProperty(replaceRule.Source, sourcePayload, replaceRule.Destination.Type);

                if (rules.Count <= 1)
                {
                    return destinationPayload.ToString(Formatting.None);
                }
            }

            if (metadata == null)
            {
                metadata = new Dictionary<string, object>();
            }

            JContainer payload = new JObject();
            foreach (var rule in rules)
            {
                if (rule.Destination.RuleAction != RuleAction.Add)
                {
                    continue;
                }

                //todo add test for this
                if (rule.Source.RuleAction == RuleAction.Route)
                {
                    continue;
                }

                object value;
                switch (rule.Source.Type)
                {
                    case DataType.Property:
                    case DataType.Model:
                        value = ModelParser.ParsePayloadProperty(rule.Source, sourcePayload, rule.Destination.Type);
                        break;

                    case DataType.HttpContent:
                        metadata.TryGetValue("HttpResponseContent", out var httpContent);
                        value = ModelParser.GetJObject(httpContent, rule.Destination.Type);
                        break;

                    case DataType.HttpStatusCode:
                        metadata.TryGetValue("HttpStatusCode", out var httpStatusCode);
                        value = ModelParser.GetJObject(httpStatusCode, rule.Destination.Type);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (string.IsNullOrWhiteSpace(rule.Destination.Path))
                {
                    payload = (JContainer)value;
                    continue;
                }

                payload.Add(new JProperty(rule.Destination.Path, value));
            }

            return payload.ToString(Formatting.None);
        }

        public HttpMethod SelectHttpVerb(WebhookConfig webhookConfig, string payload)
        {
            //build the uri from the routes first
            var routingRules = webhookConfig.WebhookRequestRules.FirstOrDefault(l => l.Routes.Any());
            if (routingRules != null)
            {
                if (routingRules.Source.Location == Location.Body)
                {
                    var path = routingRules.Source.Path;
                    var value = ModelParser.ParsePayloadPropertyAsString(path, payload);

                    if (string.IsNullOrWhiteSpace(value))
                    {
                        throw new ArgumentNullException(nameof(path), "routing path value in message payload is null or empty");
                    }

                    //selects the route based on the value found in the payload of the message
                    var rules = webhookConfig.WebhookRequestRules.FirstOrDefault(r => r.Routes.Any());
                    var route = rules?.Routes.FirstOrDefault(r => r.Selector.Equals(value, StringComparison.OrdinalIgnoreCase));
                    if (route != null)
                    {
                        return route.HttpMethod;
                    }
                    throw new Exception("route http verb mapping/selector not found between config and the properties on the domain object");
                }
            }
            return webhookConfig.HttpMethod;
        }
    }
}
