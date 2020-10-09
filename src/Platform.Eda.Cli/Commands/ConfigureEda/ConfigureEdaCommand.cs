using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor;
using Platform.Eda.Cli.Commands.ConfigureEda.OptionsValidation;
using Platform.Eda.Cli.Extensions;

namespace Platform.Eda.Cli.Commands.ConfigureEda
{
    [Command("configure-eda", Description = "Processes configuration in the provided location and calls Captain Hook API to create/update subscribers")]
    [HelpOption]
    public class ConfigureEdaCommand
    {
        private readonly IPutSubscriberProcessChain _putSubscriberProcessChain;

        public ConfigureEdaCommand(IPutSubscriberProcessChain putSubscriberProcessChain)
        {
            _putSubscriberProcessChain = putSubscriberProcessChain ?? throw new ArgumentNullException(nameof(putSubscriberProcessChain));
        }

        /// <summary>
        /// The path to the input folder containing JSON files. Can be absolute or relative.
        /// </summary>
        [Required]
        [Option("-i|--input",
            Description = "The path to the folder containing JSON files to process. Can be absolute or relative",
            ShowInHelpText = true)]
        [DirectoryExistsValidation]
        public string InputFolderPath { get; set; }

        /// <summary>
        /// The environment name.
        /// </summary>
        [Option("-e|--env",
            Description = "The environment name: (CI, TEST, PREP, SAND, PROD). Default: CI",
            ShowInHelpText = true)]
        [EnvironmentValidation]
        public string Environment { get; set; }

        /// <summary>
        /// Gets or sets a flag specifying the Dry-Run mode.
        /// </summary>
        [Option("-n|--no-dry-run",
            Description = "By default the CLI is executed in the dry-run mode where no data is passed to Captain Hook API. You can disable dry-run and allow the configuration to be applied to Captain Hook API",
            ShowInHelpText = true)]
        public bool NoDryRun { get; set; }

        /// <summary>
        /// The environment name.
        /// </summary>
        [Option("-p|--params", CommandOptionType.MultipleValue,
            Description = "The additional configuration parameters",
            ShowInHelpText = true)]
        [ReplacementParamsValidation]
        public string[] Params { get; set; }

        public async Task<int> OnExecuteAsync(IConsole console)
        {
            if (string.IsNullOrWhiteSpace(Environment))
            {
                Environment = "CI";
            }

            var replacements = BuildParametersReplacementDictionary(Params);
            var result = await _putSubscriberProcessChain.ProcessAsync(InputFolderPath, Environment, replacements, NoDryRun);

            console.WriteSuccess("Processing finished");
            return result;
        }

        private Dictionary<string, string> BuildParametersReplacementDictionary(string[] rawParams)
        {
            return rawParams?.Select(p => p.Split('=')).ToDictionary(items => items[0], items => items[1]);
        }

    }
}
