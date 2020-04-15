using CaptainHook.Cli.Extensions;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using CaptainHook.Common.Configuration;
using CaptainHook.Cli.Common;
using Newtonsoft.Json;
using System.IO.Abstractions;
using CaptainHook.Cli.Providers;
using Newtonsoft.Json.Converters;
using System.Linq;
using CaptainHook.EventHandlerActor;

namespace CaptainHook.Cli.Commands.GenerateJson
{
    /// <summary>
    /// A command to generate a set of JSON files from a Captain Hook setup powershell script
    /// </summary>
    [Command("generate-json", Description = "generates JSON files from a Captain Hook setup powershell script"), HelpOption]
    public class GenerateJsonCommand
    {
        private readonly IFileSystem fileSystem;

        private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new ShouldSerializeContractResolver(),
            Converters = new List<JsonConverter> { new StringEnumConverter() }
        };

        public GenerateJsonCommand(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        /// <summary>
        /// The path to the powershell script file (.ps1). Can be absolute or relative.
        /// </summary>
        [Option(
            Description = "The path to the powershell script file (.ps1). Can be absolute or relative.",
            ShortName = "i",
            LongName = "input",
            ShowInHelpText = true)]
        [Required]
        public string InputFilePath { get; set; }

        /// <summary>
        /// The path to the output folder that will contain the JSON files. Can be absolute or relative.
        /// </summary>
        [Option(
            Description = "The path to the output folder that will contain the JSON files. Can be absolute or relative.",
            ShortName = "o",
            LongName = "output",
            ShowInHelpText = true)]

        public string OutputFolderPath { get; set; }

        /// <summary>
        /// Command execution
        /// </summary>
        /// <param name="app">The application</param>
        /// <param name="console">Console reference</param>
        /// <returns>Return code</returns>
        public Task<int> OnExecuteAsync(CommandLineApplication app, IConsole console)
        {
            var sourceFile = Path.GetFullPath(InputFilePath);
            string outputFolder;

            if (string.IsNullOrWhiteSpace(OutputFolderPath))
            {
                outputFolder = Path.GetDirectoryName(GetType().Assembly.Location);
            }
            else
            {
                outputFolder = Path.GetFullPath(OutputFolderPath);
            }

            if(!fileSystem.File.Exists(sourceFile))
            {
                console.EmitWarning(GetType(), app.Options, $"Cannot open {InputFilePath}");

                return Task.FromResult(1);
            }

            fileSystem.Directory.CreateDirectory(outputFolder);

            var config = new ConfigurationBuilder()
                .AddEswPsFile(fileSystem, sourceFile)
                .Build();

            var values = config.GetSection("event").GetChildren().ToList();

            var endpointList = new Dictionary<string, WebhookConfig>(values.Count);

            foreach (var (configurationSection, index) in values.WithIndex())
            {
                var eventHandlerConfig = configurationSection.Get<EventHandlerConfig>();
                ConfigParser.ParseAuthScheme(eventHandlerConfig.WebhookConfig, configurationSection, "webhookconfig:authenticationconfig");

                foreach (var subscriber in eventHandlerConfig.AllSubscribers)
                {
                    var path = subscriber.WebHookConfigPath;
                    ConfigParser.ParseAuthScheme(subscriber, configurationSection, $"{path}:authenticationconfig");
                    subscriber.EventType = eventHandlerConfig.Type;
                    subscriber.PayloadTransformation = subscriber.DLQMode != null ? PayloadContractTypeEnum.WrapperContract : PayloadContractTypeEnum.Raw;

                    ConfigParser.AddEndpoints(subscriber, endpointList, configurationSection, path);

                    if (subscriber.Callback != null)
                    {
                        path = subscriber.CallbackConfigPath;
                        ConfigParser.ParseAuthScheme(subscriber.Callback, configurationSection, $"{path}:authenticationconfig");
                        subscriber.Callback.EventType = eventHandlerConfig.Type;
                        ConfigParser.AddEndpoints(subscriber.Callback, endpointList, configurationSection, path);
                    }
                }

                var jsonString = JsonConvert.SerializeObject(eventHandlerConfig, jsonSettings);
                var filename = $"event-{1 + index}-{eventHandlerConfig.Name}.json";
                fileSystem.File.WriteAllText(Path.Combine(outputFolder, filename), jsonString);

            }

            return Task.FromResult(0);
        }
    }
}
