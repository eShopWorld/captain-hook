using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CaptainHook.Cli.Extensions;
using Newtonsoft.Json.Linq;

namespace CaptainHook.Cli.Commands.GeneratePowerShell
{
    public class EventHandlerConfigToPowerShellConverter
    {
        private readonly List<PsCommand> commands = new List<PsCommand>();

        public IEnumerable<string> Convert(IEnumerable<string> eventsData)
        {
            var events = eventsData.Select(JObject.Parse);
            foreach (var (eventConfig, eventId) in events.WithIndex())
            {
                ProcessToken(eventConfig, $"event--{eventId + 1}");
            }

            return commands.Select(c => c.ToString());
        }

        private void ProcessToken(JToken property, string eventPrefix)
        {
            switch (property)
            {
                case JProperty jProperty:
                    {
                        ProcessToken(jProperty.Value, eventPrefix);
                        break;
                    }
                case JValue jValue:
                    {
                        var key = BuildKey(property, eventPrefix);
                        var value = jValue.ToString();

                        // hack for AuthenticationConfig.Type:
                        if (property.Path.EndsWith("AuthenticationConfig.Type"))
                        {
                            if (value == "OIDC")
                            {
                                commands.Add(new PsCommand(key, 2, true));
                                break;
                            }
                        }

                        if (property.Path.EndsWith("DLQMode"))
                        {
                            if (value == "WebHookMode")
                            {
                                commands.Add(new PsCommand(key, 1));
                                break;
                            }
                        }

                        commands.Add(new PsCommand(key, value));
                        break;
                    }
                case JArray jArray:
                    {
                        if (jArray.IsArrayOf(JTokenType.String))
                        {
                            var key = BuildKey(jArray, eventPrefix);
                            var value = jArray.ToValuesString();
                            commands.Add(new PsCommand(key, value));
                        }

                        foreach (var innerToken in jArray.Values())
                        {
                            ProcessToken(innerToken, eventPrefix);
                        }
                        break;
                    }
                case JObject jObject:
                    {
                        foreach (var innerToken in jObject.Values())
                        {
                            ProcessToken(innerToken, eventPrefix);
                        }
                        break;
                    }
            }
        }

        private static string BuildKey(JToken property, string currentPrefix)
        {
            string key = $"{currentPrefix}--{property.Path.Replace(".", "--").ToLower()}";
            key = Regex.Replace(key, @"(\[\d+\])", IncrementAllIndicesInKey);
            return key;
        }

        private static string IncrementAllIndicesInKey(Match match)
        {
            var rawNumber = match.Value.Trim('[', ']');
            var number = int.Parse(rawNumber);
            return $"--{++number}";
        }
    }
}