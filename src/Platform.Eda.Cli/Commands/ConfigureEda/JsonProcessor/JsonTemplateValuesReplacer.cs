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
        private static readonly Regex ValidReplacementPrefix = new Regex("^\\w+$", RegexOptions.Compiled);
        public OperationResult<string> Replace(string replacementPrefix, string fileContent, Dictionary<string, JToken> variablesDictionary)
        {
            try
            {
                if (string.IsNullOrEmpty(replacementPrefix) || !ValidReplacementPrefix.IsMatch(replacementPrefix))
                    return new CliExecutionError($"Invalid replacement prefix '{replacementPrefix}'");

                var regexPattern = new Regex($@"{{{Regex.Escape(replacementPrefix)}:([\w-]+)}}", RegexOptions.IgnoreCase | RegexOptions.Compiled);

                var vars = regexPattern.Matches(fileContent);

                var unknownVars = vars.Where(x => !variablesDictionary.ContainsKey(x.Groups[1].Value)).Select(x => x.Groups[1].Value).ToArray();
                if (unknownVars.Any())
                {
                    return new CliExecutionError(
                        $"Template has undeclared {replacementPrefix}: {string.Join(", ", unknownVars.Distinct())}");
                }

                var sb = new StringBuilder(fileContent);
                foreach (var variableMatch in vars.Reverse())
                {
                    var value = variablesDictionary[variableMatch.Groups[1].Value];

                    if (value.Type == JTokenType.String)
                    {
                        sb.Remove(variableMatch.Index, variableMatch.Length);
                        sb.Insert(variableMatch.Index, value);
                    }
                    else if (value.Type == JTokenType.Object)
                    {
                        var replacementStartIndex = variableMatch.Index - 1;
                        var replacementEndIndex = variableMatch.Index + variableMatch.Value.Length;

                        if (replacementStartIndex >= 0 && sb.Length > replacementStartIndex &&
                            replacementEndIndex >= 0 && sb.Length > replacementEndIndex) // string bounds
                        {
                            if (sb[replacementStartIndex] != '"' || sb[replacementEndIndex] != '"')
                                return new CliExecutionError(
                                    $"{replacementPrefix} replacement error. '{variableMatch.Groups[1].Value}' is defined as an object but used as value.");

                            sb.Remove(replacementStartIndex, variableMatch.Length + 2); 
                            sb.Insert(replacementStartIndex, value);
                        }
                        else
                        {
                            // Error is indices not in string bounds
                            return new CliExecutionError($@"Invalid template at {replacementPrefix} usage '{variableMatch.Value}'.");
                        }
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
