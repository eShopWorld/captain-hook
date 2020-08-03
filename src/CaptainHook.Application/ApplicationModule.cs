using Autofac;
using CaptainHook.Application.Handlers.Subscribers;
using CaptainHook.Application.Infrastructure;
using CaptainHook.Application.Infrastructure.DirectorService;
using CaptainHook.Application.Infrastructure.Mappers;
using CaptainHook.Application.Validators;

namespace CaptainHook.Application
{
    public class ApplicationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DirectorServiceGateway>().As<IDirectorServiceGateway>();
            builder.RegisterType<SubscriberEntityToConfigurationMapper>().As<ISubscriberEntityToConfigurationMapper>();

            var handlersAssembly = typeof(UpsertWebhookRequestHandler).Assembly;
            var validatorsAssembly = typeof(UpsertWebhookRequestValidator).Assembly;
            builder.RegisterMediatorInfrastructure(handlersAssembly, validatorsAssembly);
        }
    }
}
