using System;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Platform.Eda.Cli.Commands.ConfigureEda;
using Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor;
using Platform.Eda.Cli.Commands.GenerateJson;
using Platform.Eda.Cli.Commands.GeneratePowerShell;
using Platform.Eda.Cli.Extensions;

namespace Platform.Eda.Cli
{
    /// <summary>
    /// Dotnet CLI extension entry point.
    /// </summary>
    [Command(Name = "ch", Description = "Platform.Eda CLI")]
    [Subcommand(typeof(GenerateJsonCommand))]
    [Subcommand(typeof(GeneratePowerShellCommand))]
    [Subcommand(typeof(ConfigureEdaCommand))]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [ExcludeFromCodeCoverage]
    public class Program
    {
        private static IConsole _console;

        /// <summary>
        /// Gets the version of the assembly of the class calling this
        /// </summary>
        /// <returns></returns>
        private string GetVersion() => GetType().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        /// <summary>
        /// Dotnet CLI extension entry point.
        /// </summary>
        /// <param name="args">The list of arguments for this extension.</param>
        /// <returns>Executable exit code.</returns>
        public static int Main(string[] args)
        {
            CommandLineApplication app = new CommandLineApplication<Program>();

            CommandLineApplication commandParsed = null;

            app.OnParsingComplete(result =>
            {
                commandParsed = result.SelectedCommand;
            });

            _console = app.GetService<IConsole>();

            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(SetupServices());

            try
            {
                int returnCode;
                if ((returnCode = app.Execute(args)) != 0)
                {
                    var messages = new[]
                    {
                        $"WARNING: Command returned non zero code {returnCode}.",
                    }.Concat(commandParsed?.ToConsoleStrings());
                    _console.WriteWarningBox(messages.ToArray());
                }

                return returnCode;
            }
            catch (Exception exception)
            {
                var messages = new[]
                {
                    $"EXCEPTION: {exception.GetType().FullName}"
                }
                    .Concat(exception.InnerExceptions().Select(x => x.Message))
                    .Concat(commandParsed?.ToConsoleStrings());
                _console.WriteWarningBox(messages.ToArray());

                return -1;
            }
        }

        private static IServiceProvider SetupServices()
        {
            var collection = new ServiceCollection();

            collection.AddHttpClient();
            collection.AddTransient<IFileSystem, FileSystem>();
            collection.AddTransient<ISubscriberFileParser, SubscriberFileParser>();
            collection.AddTransient<IJsonTemplateValuesReplacer, JsonTemplateValuesReplacer>();
            collection.AddTransient<IJsonVarsExtractor, JsonVarsExtractor>();
            collection.AddTransient<ISubscribersDirectoryProcessor, SubscribersDirectoryProcessor>();
            collection.AddTransient<IPutSubscriberProcessChain, PutSubscriberProcessChain>();

            collection.AddSingleton(_console);
            collection.AddSingleton<BuildCaptainHookProxyDelegate>(serviceProvider =>
            {
                var clientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                return environment => ApiConsumer.BuildApiConsumer(clientFactory, environment);
            });

            return collection.BuildServiceProvider();
        }
    }
}
