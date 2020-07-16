using Autofac;
using CaptainHook.Application.Handlers.Subscribers;
using CaptainHook.Application.Infrastructure;
using CaptainHook.Application.RequestValidators;

namespace CaptainHook.Application
{
    public class ApplicationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var handlersAssembly = typeof(AddSubscriberRequestHandler).Assembly;
            var validatorsAssembly = typeof(AddSubscriberRequestValidator).Assembly;

            builder.RegisterMediatorInfrastructure(handlersAssembly, validatorsAssembly);
        }
    }
}
