using System.Collections.Generic;
using CaptainHook.Domain.Results;
using Newtonsoft.Json.Linq;

namespace Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor
{
    public interface IJsonVarsExtractor
    {
        /// <summary>
        /// Removes the vars node from <see cref="fileContent"/> and
        /// returns a dictionary of variables for the <see cref="environmentName"/> environment.
        /// </summary>
        /// <param name="fileContent"></param>
        /// <param name="environmentName"></param>
        /// <returns></returns>
        public OperationResult<Dictionary<string, JToken>> ExtractVars(JObject fileContent, string environmentName);
    }
}