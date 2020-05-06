using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CaptainHook.Cli.Commands.GeneratePowerShell.Internal;
using Newtonsoft.Json.Linq;

namespace CaptainHook.Cli.Commands.GeneratePowerShell
{
    public class EventHandlerConfigToPowerShellConverter
    {
        private readonly PsCommandList commands = new PsCommandList();

        public IEnumerable<string> Convert(IEnumerable<string> eventsData)
        {
            int eventId = 1;

            const string eventPrefix = "event";

            var events = eventsData.Select(JObject.Parse);
            foreach (var eventConfig in events)
            {
                var currentItemPrefix = $"{eventPrefix}--{eventId}";

                var allTokens = eventConfig.Values().ToList();

                foreach (var property in allTokens)
                {
                    ProcessToken(property, currentItemPrefix);
                }

                eventId++;
            }

            return commands.ToCommandLines();
        }

        private void ProcessToken(JToken property, string currentPrefix)
        {
            switch (property)
            {
                case JProperty jProperty:
                    {
                        ProcessToken(jProperty.Value, currentPrefix);
                        break;
                    }
                case JValue jValue:
                    {
                        string key = $"{currentPrefix}--{property.Path.Replace(".", "--").ToLower()}";
                        key = Regex.Replace(key, @"(\[\d+\])", FixKeyIndices);

                        var value = jValue.ToString();

                        // hack for AuthenticationConfig.Type:
                        if (property.Path.EndsWith("AuthenticationConfig.Type"))
                        {
                            if (value == "OIDC")
                            {
                                commands.Add(key, 2, true);
                                break;
                            }
                        }

                        if (property.Path.EndsWith("DLQMode"))
                        {
                            if (value == "WebHookMode")
                            {
                                commands.Add(key, 1);
                                break;
                            }
                        }

                        commands.Add(key, value);
                        break;
                    }
                case JArray jArray:
                    {
                        // hack for AuthenticationConfig.Scopes:
                        if (property.Path.EndsWith("AuthenticationConfig.Scopes"))
                        {
                            string key = $"{currentPrefix}--{property.Path.Replace(".", "--").ToLower()}";
                            key = Regex.Replace(key, @"(\[\d+\])", FixKeyIndices);

                            var strings = property.Values().OfType<JValue>().Select(v => v.Value.ToString());
                            commands.Add(key, strings);
                            break;
                        }

                        //for (int i = 1; i <= jArray.Count; i++)
                        //{
                        //    string newPrefix = $"{currentPrefix}--{property.Name.ToLower()}--{i}";
                        //    ProcessToken(jArray[i].V, newPrefix);

                        //}

                        int arrayIndex = 1;
                        var allTokens = jArray.Values().ToList();
                        foreach (var innerToken in allTokens)
                        {
                            //string newPrefix = $"{currentPrefix}--{property.Path.Replace(".", "--").ToLower()}--{arrayIndex}";
                            ProcessToken(innerToken, currentPrefix);
                        }

                        break;
                    }
                case JObject jObject:
                    {
                        //string newPrefix = $"{currentPrefix}--{property.Path.Replace(".", "--").ToLower()}";

                        var allTokens = jObject.Values().ToList();
                        foreach (var innerToken in allTokens)
                        {
                            ProcessToken(innerToken, currentPrefix);
                        }
                        break;
                    }
            }
        }

        private string FixKeyIndices(Match match)
        {
            var rawNumber = match.Value.Trim('[', ']');
            var number = int.Parse(rawNumber);
            return $"--{++number}";
        }
    }
}