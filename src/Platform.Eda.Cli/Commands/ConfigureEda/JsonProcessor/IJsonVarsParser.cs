using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor
{
    public interface IJsonVarsParser
    {
        public Dictionary<string, JToken> GetFileVars(JObject fileContent, string environmentName);
    }
}