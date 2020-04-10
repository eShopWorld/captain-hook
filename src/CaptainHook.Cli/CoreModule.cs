using Autofac;
using CaptainHook.Cli.Services;

namespace CaptainHook.Cli
{
    public class CoreModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            //var config = EswDevOpsSdk.BuildConfiguration();
            //builder.ConfigureTelemetryKeys(config["BBInstrumentationKey"], config["BBInstrumentationKey"]);
            //builder.RegisterModule<TelemetryModule>();
            //builder.RegisterType<TelemetryClient>().SingleInstance();
            builder.RegisterType<PathService>().SingleInstance();
        }
    }
}
