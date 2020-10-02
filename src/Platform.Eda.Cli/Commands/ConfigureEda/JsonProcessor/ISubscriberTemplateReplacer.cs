using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor
{
    public interface ISubscriberTemplateReplacer
    {
        public JObject Replace(JObject fileContent, Dictionary<string, JToken> variables);
    }
}