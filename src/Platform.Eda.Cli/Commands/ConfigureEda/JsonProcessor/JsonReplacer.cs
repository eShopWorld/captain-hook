using CaptainHook.Domain.Results;
using Newtonsoft.Json.Linq;
using Platform.Eda.Cli.Common;
using System;
using System.Collections.Generic;

namespace Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor
{
    public class JsonReplacer : IJsonReplacer
    {
        public OperationResult<JObject> Replace(JObject source, Dictionary<string, JToken> replacements)
        {
            var tokens = GetTokens(source);

            try
            {
                foreach (var token in tokens)
                {
                    ReplaceProperties(token, replacements);
                }
            }
            catch(Exception exception)
            {
                return new CliExecutionError(exception.Message);
            }
            return source;
        }

        private void ReplaceProperties(JToken token, Dictionary<string, JToken> replacements)
        {
            if (token.Type == JTokenType.Property && ((JProperty)token).Value.Type == JTokenType.String)
            {
                var property = (JProperty)token;

                var value = (string)property.Value;
                foreach (var replacement in replacements)
                {
                    if (value.Contains(replacement.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        if (replacement.Value.Type == JTokenType.String)
                        {
                            var replacedValue = value.Replace(replacement.Key, $"{(string)replacement.Value}");
                            property.Value = (JToken)replacedValue;
                        }
                        if (replacement.Value.Type == JTokenType.Object)
                        {
                            property.Value = replacement.Value;
                        }
                    }
                }
            }
        }

        private IEnumerable<JToken> GetTokens(JObject obj)
        {
            var toSearch = new Stack<JToken>(obj.Children());
            while (toSearch.Count > 0)
            {
                var inspected = toSearch.Pop();
                yield return inspected;
                foreach (var child in inspected)
                {
                    toSearch.Push(child);
                }
            }
        }
    }
}
