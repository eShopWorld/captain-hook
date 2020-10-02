using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor
{
    public class JsonVarsParser : IJsonVarsParser
    {
        public Dictionary<string, JToken> GetFileVars(JObject fileContent, string environmentName)
        {
            if (!fileContent.ContainsKey("vars")) 
                return new Dictionary<string, JToken>(); // no vars
            
            var varsDict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, JToken>>>(fileContent["vars"]!.ToString());
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
                        }
                    }
                }
            }

            fileContent.Remove("vars");
            return outputVarsDict;
        }
    }
}
