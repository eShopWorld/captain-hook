using System;
using System.Collections.Generic;
using System.Linq;
using CaptainHook.Domain.Results;
using Newtonsoft.Json.Linq;
using Platform.Eda.Cli.Common;

namespace Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor
{
    public class EnvironmentNamesExtractor
    {
        public OperationResult<IEnumerable<string>> Find(JObject varsJObject)
        {
            if (varsJObject == null)
                return new HashSet<string>();

            Dictionary<string, Dictionary<string, JToken>> varsDict;
            try
            {
                varsDict = varsJObject.ToObject<Dictionary<string, Dictionary<string, JToken>>>();
            }
            catch (Exception e)
            {
                return new CliExecutionError($"Cannot parse vars. {e.Message}.");
            }

            if (varsDict == null)
                return new CliExecutionError($"Cannot parse vars from {varsJObject}.");

            var environmentNames = varsDict.SelectMany(var => var.Value).SelectMany(env => env.Key.Split(',')).Select(name => name.ToLower()).Distinct().ToList();

            var unknownNames = environmentNames.Where(name => !ConfigureEdaConstants.EnvironmentNames.Contains(name)).ToList();
            if (unknownNames.Any())
            {
                return new CliExecutionError($"File contains unknown envs names: {string.Join(',', unknownNames)}");
            }

            return environmentNames;
        }
    }
}