using Autofac;
using Autofac.Extensions.DependencyInjection;
using CaptainHook.Cli.Commands.GenerateJson;
using CaptainHook.Cli.Extensions;
using CaptainHook.Cli.Telemetry;
using Eshopworld.Core;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace CaptainHook.Cli
{
    /// <summary>
    /// Dotnet CLI extension entry point.
    /// </summary>
    [Command(Name = "ch", Description = "CaptainHook CLI")]
    [Subcommand(typeof(GenerateJsonCommand))]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    public class Program
    {
        private static IBigBrother bigBrother;
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

            CaptainHookCliCommandExecutionTimedEvent timedEvent = null;
            app.OnParsingComplete(result =>
            {
                commandParsed = result.SelectedCommand;
                timedEvent = new CaptainHookCliCommandExecutionTimedEvent
                {
                    CommandType = commandParsed?.GetType().FullName ?? app.GetType().FullName,
                    Arguments = commandParsed?.Options.ToConsoleString()
                };
            });

            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(SetupAutofac());

            try
            {
                bigBrother = app.GetService<IBigBrother>();
                console = app.GetService<IConsole>();
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
                    bigBrother,
                    exception,
                    commandParsed?.GetType() ?? app.GetType(),
                    commandParsed?.Options);

                return -1;
            }
            finally
            {
                if (timedEvent != null)
                {
                    bigBrother?.Publish(timedEvent);
                    bigBrother?.Flush();
                }
            }
        }

        private static IServiceProvider SetupAutofac()
        {
            var serviceProviderFactory = new AutofacServiceProviderFactory();

            var builder = new ContainerBuilder();
            builder.RegisterAssemblyModules(Assembly.GetExecutingAssembly());

            return serviceProviderFactory.CreateServiceProvider(builder);
        }
    }
}
