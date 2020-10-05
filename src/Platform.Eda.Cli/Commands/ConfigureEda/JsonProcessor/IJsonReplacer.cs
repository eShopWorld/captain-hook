using CaptainHook.Domain.Results;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor
{
    public interface IJsonReplacer
    {
        /// <summary>
        /// Replace a dictionary of keys into the source JSON object
        /// </summary>
        /// <param name="source">The source JSON object</param>
        /// <param name="replacements">A dictionary of replacements</param>
        /// <returns></returns>
        public OperationResult<JObject> Replace(JObject source, Dictionary<string, JToken> replacements);
    }
}
