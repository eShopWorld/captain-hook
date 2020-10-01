using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using CaptainHook.Domain.Results;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;
using Platform.Eda.Cli.Common;

namespace Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor
{
    public class SubscribersDirectoryProcessor : ISubscribersDirectoryProcessor
    {
        private readonly IFileSystem _fileSystem;

        public SubscribersDirectoryProcessor(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public OperationResult<IEnumerable<PutSubscriberFile>> ProcessDirectory(string inputFolderPath)
        {
            var sourceFolderPath = Path.GetFullPath(inputFolderPath);
            if (!_fileSystem.Directory.Exists(sourceFolderPath))
            {
                return new CliExecutionError($"Cannot open {inputFolderPath}");
            }

            var subscribers = new List<PutSubscriberFile>();

            try
            {
                var subscriberFiles = _fileSystem.Directory.GetFiles(sourceFolderPath, "*.json", SearchOption.AllDirectories);
                subscribers.AddRange(subscriberFiles.Select(ProcessFile));

                if (!subscribers.Any())
                {
                    return new CliExecutionError($"No files have been found in '{sourceFolderPath}'");
                }
            }
            catch (Exception e)
            {
                return new CliExecutionError(e.ToString());
            }

            return subscribers;
        }

        private PutSubscriberFile ProcessFile(string fileName)
        {
            try
            {
                var content = _fileSystem.File.ReadAllText(fileName);
                var contentJObject = JObject.Parse(content);
                var varsDictionary = GetFileVars(contentJObject);

                var request = JsonConvert.DeserializeObject<PutSubscriberRequest>(contentJObject.ToString());

                return new PutSubscriberFile
                {
                    File = new FileInfo(fileName),
                    Request = request
                };
            }
            catch (Exception ex)
            {
                return new PutSubscriberFile
                {
                    File = new FileInfo(fileName),
                    Error = ex.Message
                };
            }
        }

        public static Dictionary<string, Dictionary<string, string>> GetFileVars(JObject fileContent)
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