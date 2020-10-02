using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor
{
    public class SubscriberVarsReplacer : ISubscriberVarsReplacer
    {
        public JObject ReplaceVars(JObject fileContent, Dictionary<string, JToken> variables)
        {
            StringBuilder sb = new StringBuilder(fileContent.ToString());
            foreach (var (propertyKey, envDictionary) in variables)
            {
                var variableName = $"{{vars:{propertyKey}}}";
                var variableNameWholeValue = $@"""{variableName}""";
                
                sb.Replace(envDictionary.Type == JTokenType.String ? variableName : variableNameWholeValue,
                    envDictionary.ToString());
            }

            return JObject.Parse(sb.ToString());
        }
    }
}
