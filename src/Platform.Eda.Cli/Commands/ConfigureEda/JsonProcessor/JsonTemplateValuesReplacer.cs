using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CaptainHook.Domain.Results;
using Newtonsoft.Json.Linq;
using Platform.Eda.Cli.Common;

namespace Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor
{
    public class JsonTemplateValuesReplacer : IJsonTemplateValuesReplacer
    {
        private static readonly Dictionary<TemplateReplacementType, string> ReplacementTypeToPrefix = new Dictionary<TemplateReplacementType, string>
        {
            {TemplateReplacementType.Params, "params"},
            {TemplateReplacementType.Vars, "vars"}
        };

        private static readonly Dictionary<TemplateReplacementType, Regex> ReplacementTypeToRegex = new Dictionary<TemplateReplacementType, Regex>
        {
            {TemplateReplacementType.Params, new Regex($@"{{params:([a-zA-Z]+[a-zA-Z0-9-_]+)}}", RegexOptions.Compiled)},
            {TemplateReplacementType.Vars, new Regex($@"{{vars:([a-zA-Z]+[a-zA-Z0-9-_]+)}}", RegexOptions.Compiled)}
        };

        public OperationResult<string> Replace(TemplateReplacementType replacementType, string fileContent, Dictionary<string, JToken> variablesDictionary)
        {
            try
            {
                var sb = new StringBuilder(fileContent);

                var replacementPrefix = ReplacementTypeToPrefix[replacementType];
                var regexPattern = ReplacementTypeToRegex[replacementType];

                var vars = regexPattern.Matches(fileContent);

                var tempDictionary =
                    variablesDictionary.ToDictionary(k => $"{{{replacementPrefix}:{k.Key}}}", k => k.Value);

                var unknownVars = vars.Where(x => !tempDictionary.ContainsKey(x.Value)).Select(x => x.Value).ToArray();
                if (unknownVars.Any())
                {
                    return new CliExecutionError(
                        $"Template has an undeclared {replacementPrefix}: {string.Join(',', unknownVars)}");
                }

                foreach (var variableMatch in vars.Reverse())
                {
                    var value = tempDictionary[variableMatch.Value];

                    if (value.Type == JTokenType.String)
                    {
                        sb.Replace(variableMatch.Value, value.ToString());
                    }
                    else if (value.Type == JTokenType.Object)
                    {
                        if (sb[variableMatch.Index - 1] != '"' || sb[variableMatch.Index + variableMatch.Value.Length] != '"')
                            return new CliExecutionError(
                                $"{replacementType} replacement error. '{variableMatch.Groups[1]}' is defined as an object but used as value.");

                        sb.Replace($"\"{variableMatch.Value}\"", value.ToString());
                    }
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return new CliExecutionError(ex.Message);
            }
        }
    }
}
