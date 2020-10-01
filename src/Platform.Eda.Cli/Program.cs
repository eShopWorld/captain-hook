﻿using System;
using System.IO.Abstractions;
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
    [Command(Name = "ch", Description = "CaptainHook CLI")]
    [Subcommand(typeof(GenerateJsonCommand))]
    [Subcommand(typeof(GeneratePowerShellCommand))]
    [Subcommand(typeof(ConfigureEdaCommand))]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    public class Program
    {
        private static IConsole console;

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

            console = app.GetService<IConsole>();
            
            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(SetupServices());

            try
            {
                int returnCode;
                if ((returnCode = app.Execute(args)) != 0)
                {
                    console.EmitWarning(
                        commandParsed?.GetType() ?? app.GetType(),
                        commandParsed?.Options,
                        $"Command returned non zero code {returnCode}.");
                }

                return returnCode;
            }
            catch (Exception exception)
            {
                console.EmitException(
                    exception,
                    commandParsed?.GetType() ?? app.GetType(),
                    commandParsed?.Options);

                return -1;
            }
        }

        private static IServiceProvider SetupServices()
        {
            var collection = new ServiceCollection();

            collection.AddHttpClient();
            collection.AddTransient<IFileSystem, FileSystem>();
            collection.AddTransient<IConsoleSubscriberWriter, ConsoleSubscriberWriter>();
            collection.AddTransient<ISubscribersDirectoryProcessor, SubscribersDirectoryProcessor>();

            collection.AddSingleton(console);
            collection.AddSingleton<BuildCaptainHookProxyDelegate>(serviceProvider =>
            {
                var clientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                return environment => ApiConsumer.BuildApiConsumer(clientFactory, environment);
            });

            return collection.BuildServiceProvider();
        }
    }
}
