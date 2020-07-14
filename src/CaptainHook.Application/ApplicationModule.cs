using System.Reflection;
using Autofac;
using CaptainHook.Application.Handlers.Subscribers;
using CaptainHook.Application.Infrastructure;
using CaptainHook.Application.RequestValidators;
using FluentValidation;
using MediatR;

namespace CaptainHook.Application
{
    public class ApplicationModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<Mediator>()
                .As<IMediator>()
                .InstancePerLifetimeScope();

            builder.Register<ServiceFactory>(context =>
            {
                var c = context.Resolve<IComponentContext>();
                return t => c.Resolve(t);
            });

            builder
                .RegisterAssemblyTypes(typeof(AddSubscriberRequestHandler).GetTypeInfo().Assembly)
                .AsClosedTypesOf(typeof(IRequestHandler<,>))
                .AsImplementedInterfaces();

            var validatorsInAssembly = AssemblyScanner.FindValidatorsInAssembly(typeof(AddSubscriberRequestValidator).Assembly);
            foreach (var validator in validatorsInAssembly)
            {
                builder.RegisterType(validator.ValidatorType).As(validator.InterfaceType);
            }

            builder.RegisterGeneric(typeof(ValidatorPipelineBehavior<,>)).As(typeof(IPipelineBehavior<,>));
        }
    }
}
