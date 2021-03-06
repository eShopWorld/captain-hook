﻿using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using CaptainHook.Common.Configuration;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Platform.Eda.Cli.Common;
using Platform.Eda.Cli.Extensions;
using Platform.Eda.Cli.Providers;

namespace Platform.Eda.Cli.Commands.GenerateJson
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
            Converters = { new StringEnumConverter() }
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

            if (!fileSystem.File.Exists(sourceFile))
            {
                console.WriteWarning($"Cannot open {InputFilePath}");

                return Task.FromResult(1);
            }

            fileSystem.Directory.CreateDirectory(outputFolder);

            var config = new ConfigurationBuilder()
                .AddEswPsFile(fileSystem, sourceFile)
                .Build();

            var values = config.GetSection("event").GetChildren().ToList();

            foreach (var (configurationSection, index) in values.WithIndex())
            {
                var eventHandlerConfig = configurationSection.Get<EventHandlerConfig>();
                ConfigParser.ParseAuthScheme(eventHandlerConfig.WebhookConfig, configurationSection, "webhookconfig:authenticationconfig");

                UpdateWebhookRulesRoutesAuthenticationConfig(eventHandlerConfig, configurationSection);
                UpdateSubscribersData(eventHandlerConfig, configurationSection);

                var jsonString = JsonConvert.SerializeObject(eventHandlerConfig, jsonSettings);
                var filename = $"event-{1 + index}-{eventHandlerConfig.Name}.json";
                fileSystem.File.WriteAllText(Path.Combine(outputFolder, filename), jsonString);
            }

            return Task.FromResult(0);
        }

        private static void UpdateWebhookRulesRoutesAuthenticationConfig(EventHandlerConfig eventHandlerConfig, IConfigurationSection configurationSection)
        {
            var webhookConfig = eventHandlerConfig.WebhookConfig;
            for (var i = 0; i < webhookConfig.WebhookRequestRules.Count; i++)
            {
                var webhookRequestRule = webhookConfig.WebhookRequestRules[i];
                for (var y = 0; y < webhookRequestRule.Routes.Count; y++)
                {
                    var route = webhookRequestRule.Routes[y];
                    if (string.IsNullOrWhiteSpace(route.Uri))
                    {
                        continue;
                    }

                    var authPath = $"webhookconfig:webhookrequestrules:{i + 1}:routes:{y + 1}:authenticationconfig";
                    ConfigParser.ParseAuthScheme(route, configurationSection, authPath);
                }
            }
        }

        private static void UpdateSubscribersData(EventHandlerConfig eventHandlerConfig, IConfigurationSection configurationSection)
        {
            foreach (var subscriber in eventHandlerConfig.AllSubscribers)
            {
                var path = subscriber.WebHookConfigPath;
                ConfigParser.ParseAuthScheme(subscriber, configurationSection, $"{path}:authenticationconfig");
                subscriber.EventType = eventHandlerConfig.Type;
                subscriber.PayloadTransformation = subscriber.DLQMode != null
                    ? PayloadContractTypeEnum.WrapperContract
                    : PayloadContractTypeEnum.Raw;

                ConfigParser.AddEndpoints(subscriber, configurationSection, path);

                if (subscriber.Callback != null)
                {
                    path = subscriber.CallbackConfigPath;
                    ConfigParser.ParseAuthScheme(subscriber.Callback, configurationSection, $"{path}:authenticationconfig");
                    subscriber.Callback.EventType = eventHandlerConfig.Type;
                    ConfigParser.AddEndpoints(subscriber.Callback, configurationSection, path);
                }
            }
        }
    }
}
