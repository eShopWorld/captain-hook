using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CaptainHook.Domain.Results;
using Newtonsoft.Json.Linq;
using Platform.Eda.Cli.Common;

namespace Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor
{
    public class JsonTemplateValuesValuesReplacer : IJsonTemplateValuesReplacer
    {
        private static readonly Dictionary<TemplateReplacementType, string> ReplacementTypeToPrefix = new Dictionary<TemplateReplacementType, string>
        {
            {TemplateReplacementType.Params, "params"},
            {TemplateReplacementType.Vars, "vars"}
        };

        public OperationResult<string> Replace(TemplateReplacementType replacementType, string fileContent, Dictionary<string, JToken> variablesDictionary)
        {
            var sb = new StringBuilder(fileContent);

            var replacementPrefix = ReplacementTypeToPrefix[replacementType];

            var regexPattern = $@"{{{replacementPrefix}:[a-zA-Z]+[a-zA-Z0-9-_]+}}";
            var vars = Regex.Matches(fileContent, regexPattern);

            var tempDictionary = variablesDictionary.ToDictionary(k => $"{{{replacementPrefix}:{k.Key}}}", k => k.Value);

            var unknownVars = vars.Where(x => !tempDictionary.ContainsKey(x.Value)).Select(x => x.Value).ToArray();
            if (unknownVars.Any())
            {
                return new CliExecutionError($"Template has an undeclared {replacementType}: {string.Join(',', unknownVars)}");
            }

            foreach (Match variableMatch in vars)
            {
                var value = tempDictionary[variableMatch.Value];

                if (value.Type == JTokenType.String)
                {
                    sb = sb.Replace(variableMatch.Value, value.ToString());
                }
                else if (value.Type == JTokenType.Object)
                {
                    sb = sb.Replace($"\"{variableMatch.Value}\"", value.ToString());
                }
            }

            return sb.ToString();
        }
    }
}
