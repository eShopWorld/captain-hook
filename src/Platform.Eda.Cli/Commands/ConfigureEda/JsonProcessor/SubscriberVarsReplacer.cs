using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor
{
    public class SubscriberVarsReplacer : ISubscriberVarsReplacer
    {
        public JObject ReplaceVars(JObject fileContent, Dictionary<string, Dictionary<string, string>> variables)
        {
            foreach (var (propertyKey, envDictionary) in variables)
            {
                foreach (var (envKey, value) in envDictionary)
                {
                    string variableName = $"vars:{propertyKey}.{envKey}";
                    var references = fileContent.SelectTokens($"$..[*{variableName}]");
                }

            }
            throw new NotImplementedException();
        }
    }
}
