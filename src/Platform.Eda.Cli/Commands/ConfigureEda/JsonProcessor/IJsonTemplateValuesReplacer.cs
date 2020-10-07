using System.Collections.Generic;
using CaptainHook.Domain.Results;
using Newtonsoft.Json.Linq;

namespace Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor
{
    public interface IJsonTemplateValuesReplacer
    {
        /// <summary>
        /// Replaces all occurences <see cref="variablesDictionary"/> keys in <see cref="fileContent"/>
        /// </summary>
        /// <param name="replacementPrefix">vars or params</param>
        /// <param name="fileContent">A valid JSON template string</param>
        /// <param name="variablesDictionary">Dictionary of vars/params names mapped to string/object JToken values</param>
        /// <returns></returns>
        public OperationResult<string> Replace(string replacementPrefix, string fileContent, Dictionary<string, JToken> variablesDictionary);
    }
}