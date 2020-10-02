using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor
{
    public class JsonVarsParser : IJsonVarsParser
    {
        public Dictionary<string, string> GetFileVars(JObject fileContent, string environmentName)
        {
            if (fileContent.ContainsKey("vars"))
            {
                var varsDict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, JToken>>>(fileContent["vars"]!.ToString());

                var outputVarsDict = new Dictionary<string, string>();

                foreach (var (propertyKey, innerDict) in varsDict)
                {
                    foreach (var (envKey, val) in innerDict)
                    {
                        var stringVal = val.Type == JTokenType.String ? val.ToString() : val.ToString(Formatting.None);
                        
                        if (string.Equals(environmentName, envKey, StringComparison.OrdinalIgnoreCase))
                        {
                            outputVarsDict[propertyKey] = stringVal;
                        }
                        else if (envKey.Contains(","))
                        {
                            foreach (var singleEnv in envKey.Split(','))
                            {
                                if (string.Equals(environmentName, singleEnv, StringComparison.OrdinalIgnoreCase))
                                    outputVarsDict[propertyKey] = stringVal;
                            }
                        }
                    }
                }

                fileContent.Remove("vars");
                return outputVarsDict;
            }
            return new Dictionary<string, string>(); // no vars
        }
    }
}
