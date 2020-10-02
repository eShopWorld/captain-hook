using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor
{
    public class SubscriberTemplateReplacer : ISubscriberTemplateReplacer
    {
        private readonly string _replacementPrefix;

        private static readonly Dictionary<TemplateReplacementType, string> ReplacementTypeToPrefix = new Dictionary<TemplateReplacementType, string>
        {
            {TemplateReplacementType.Params, "params"},
            {TemplateReplacementType.Vars, "vars"}
        };

        public SubscriberTemplateReplacer(TemplateReplacementType replacementType)
        {
            _replacementPrefix = ReplacementTypeToPrefix[replacementType];
        }

        public JObject Replace(JObject fileContent, Dictionary<string, JToken> variables)
        {
            var sb = new StringBuilder(fileContent.ToString());

            foreach (var (propertyKey, val) in variables)
            {
                var variableName = $"{{{_replacementPrefix}:{propertyKey}}}";
                var variableNameWholeValue = $@"""{variableName}""";
                
                sb.Replace(val.Type == JTokenType.String ? variableName : variableNameWholeValue,
                    val.ToString());
            }

            return JObject.Parse(sb.ToString());
        }
    }
}
