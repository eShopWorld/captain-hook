using System;
using System.IO.Abstractions;
using CaptainHook.Domain.Results;
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
    }
}