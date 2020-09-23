using System;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Platform.Eda.Cli.Commands.ConfigureEda;
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

            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(SetupAutofac());

            try
            {
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
                    exception,
                    commandParsed?.GetType() ?? app.GetType(),
                    commandParsed?.Options);

                return -1;
            }
        }

        private static IServiceProvider SetupAutofac()
        {
            var serviceProviderFactory = new AutofacServiceProviderFactory();

            var builder = new ContainerBuilder();
            builder.RegisterAssemblyModules(Assembly.GetExecutingAssembly());

            builder.RegisterType<SubscribersDirectoryProcessor>().As<ISubscribersDirectoryProcessor>();
            builder.RegisterType<ApiConsumer>().As<IApiConsumer>();
            
            builder.RegisterInstance(new ApiClientFixture().GetApiClient());
            
            return serviceProviderFactory.CreateServiceProvider(builder);
        }
    }
}
