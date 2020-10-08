using System;
using System.Collections.Generic;
using CaptainHook.Domain.Results;
using Newtonsoft.Json.Linq;
using Platform.Eda.Cli.Common;

namespace Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor
{
    public class JsonVarsExtractor : IJsonVarsExtractor
    {
        public OperationResult<Dictionary<string, JToken>> ExtractVars(JObject varsJObject, string environmentName)
        {
            Dictionary<string, Dictionary<string, JToken>> varsDict;

            if (varsJObject == null)
                return new Dictionary<string, JToken>(); // no vars


            if (!ConfigureEdaConstants.EnvironmentNames.Contains(environmentName))
                return new CliExecutionError($"Cannot extract vars for environment '{environmentName}'.");

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

            var outputVarsDict = new Dictionary<string, JToken>();
            foreach (var (propertyKey, innerDict) in varsDict)
            {
                foreach (var (envKey, val) in innerDict)
                {
                    if (string.Equals(environmentName, envKey, StringComparison.OrdinalIgnoreCase))
                    {
                        outputVarsDict[propertyKey] = val;
                    }
                    else if (envKey.Contains(","))
                    {
                        foreach (var singleEnv in envKey.Split(','))
                        {
                            if (string.Equals(environmentName, singleEnv, StringComparison.OrdinalIgnoreCase))
                                outputVarsDict[propertyKey] = val;
                            else if (!ConfigureEdaConstants.EnvironmentNames.Contains(singleEnv))
                                return new CliExecutionError($"Unsupported environment '{singleEnv}' while parsing vars.");
                        }
                    }
                    else if (!ConfigureEdaConstants.EnvironmentNames.Contains(envKey))
                        return new CliExecutionError($"Unsupported environment '{envKey}' while parsing vars.");
                }
            }

            return outputVarsDict;
        }
    }
}