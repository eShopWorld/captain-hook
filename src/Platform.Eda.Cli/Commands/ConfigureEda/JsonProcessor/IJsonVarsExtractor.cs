using System.Collections.Generic;
using CaptainHook.Domain.Results;
using Newtonsoft.Json.Linq;

namespace Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor
{
    public interface IJsonVarsExtractor
    {
        /// <summary>
        /// Deserializes the Json Object and returns a dictionary of variables for the <see cref="environmentName"/> environment.
        /// </summary>
        /// <param name="varsJObject"></param>
        /// <param name="environmentName"></param>
        /// <returns></returns>
        public OperationResult<Dictionary<string, JToken>> ExtractVars(JObject varsJObject, string environmentName);
    }
}