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
                        commands.Add(new PsCommand(key, jValue));
                        break;
                    }
                case JArray jArray:
                    {
                        if (jArray.IsArrayOf(JTokenType.String))
                        {
                            var key = BuildKey(jArray, eventPrefix);
                            commands.Add(new PsCommand(key, jArray.ToValuesString()));
                            break;
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