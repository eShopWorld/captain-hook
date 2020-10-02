using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor
{
    interface ISubscriberVarsReplacer
    {
        public JObject ReplaceVars(JObject fileContent, Dictionary<string, Dictionary<string, string>> variables);
    }
}