using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Platform.Eda.Cli.Extensions;

namespace Platform.Eda.Cli.Commands.GeneratePowerShell
{
    public class EventHandlerConfigToPowerShellConverter
    {
        private readonly List<PsCommand> commands = new List<PsCommand>();

        public IEnumerable<string> Convert(SortedDictionary<int, string> eventsData)
        {
            foreach (var (eventId, content) in eventsData)
            {
                var eventConfig = JObject.Parse(content);
                ProcessToken(eventConfig, $"event--{eventId}");
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