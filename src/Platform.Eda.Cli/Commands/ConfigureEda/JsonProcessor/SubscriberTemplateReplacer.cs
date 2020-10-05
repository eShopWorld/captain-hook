using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using CaptainHook.Domain.Results;
using Newtonsoft.Json.Linq;
using Platform.Eda.Cli.Common;

namespace Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor
{
    public class SubscriberTemplateReplacer : ISubscriberTemplateReplacer
    {
        private static readonly Dictionary<TemplateReplacementType, string> ReplacementTypeToPrefix = new Dictionary<TemplateReplacementType, string>
        {
            {TemplateReplacementType.Params, "params"},
            {TemplateReplacementType.Vars, "vars"}
        };

        public OperationResult<string> Replace(TemplateReplacementType replacementType, string fileContent, Dictionary<string, JToken> variables)
        {
            var replacementPrefix = ReplacementTypeToPrefix[replacementType];
            var sb = new StringBuilder(fileContent.ToString());

            foreach (var (propertyKey, val) in variables)
            {
                var variableName = $"{{{replacementPrefix}:{propertyKey}}}";
                var variableNameWholeValue = $@"""{variableName}""";

                sb.Replace(val.Type == JTokenType.String ? variableName : variableNameWholeValue,
                    val.ToString());
            }

            return sb.ToString();
        }

        private bool IsSurroundedByQuotes(string variableMatchValue)
        {
            throw new NotImplementedException();
        }
    }
}
