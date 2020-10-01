﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using CaptainHook.Domain.Results;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Platform.Eda.Cli.Common;

namespace Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor
{
    public class SubscriberFileParser : ISubscriberFileParser
    {
        private readonly IFileSystem _fileSystem;

        public SubscriberFileParser(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public OperationResult<JObject> ParseFile(string fileName)
        {
            try
            {
                var content = _fileSystem.File.ReadAllText(fileName);
                var contentJObject = JObject.Parse(content);
                return contentJObject;
            }
            catch (Exception ex)
            {
                return new CliExecutionError(ex.Message);
            }
        }

        public Dictionary<string, Dictionary<string, string>> GetFileVars(JObject fileContent)
        {
            if (fileContent.ContainsKey("vars"))
            {
                var varsDict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, JToken>>>(fileContent["vars"]!.ToString());

                var outputVarsDict = new Dictionary<string, Dictionary<string, string>>();

                foreach (var (propertyKey, innerDict) in varsDict)
                {
                    outputVarsDict[propertyKey] = new Dictionary<string, string>();
                    foreach (var (envKey, val) in innerDict)
                    {
                        var stringVal = val.Type == JTokenType.String ? val.ToString() : val.ToString(Formatting.None);
                        if (envKey.Contains(","))
                        {
                            foreach (var singleEnv in envKey.Split(','))
                            {
                                outputVarsDict[propertyKey][singleEnv] = stringVal;
                            }
                        }
                        else // convert to string
                        {
                            outputVarsDict[propertyKey][envKey] = stringVal;
                        }
                    }
                }

                fileContent.Remove("vars");
                return outputVarsDict;
            }
            return new Dictionary<string, Dictionary<string, string>>(); // no vars
        }
    }
}