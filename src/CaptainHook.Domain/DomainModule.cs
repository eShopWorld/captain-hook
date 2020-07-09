using Autofac;
using CaptainHook.Domain.Handlers.Subscribers;
using CaptainHook.Domain.RequestValidators;
using FluentValidation;
using MediatR;
using System.Reflection;

namespace CaptainHook.Domain
{
    public class DomainModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Mediator itself
            builder
                .RegisterType<Mediator>()
                .As<IMediator>()
                .InstancePerLifetimeScope();

            // request & notification handlers
            builder.Register<ServiceFactory>(context =>
            {
                var c = context.Resolve<IComponentContext>();
                return t => c.Resolve(t);
            });

            builder
                .RegisterAssemblyTypes(typeof(AddSubscriberRequestHandler).GetTypeInfo().Assembly)
                .AsClosedTypesOf(typeof(IRequestHandler<,>))
                .AsImplementedInterfaces();

            //builder.RegisterAssemblyTypes(typeof(AddSubscriberRequestHandler).GetTypeInfo().Assembly).AsImplementedInterfaces();

            // For all the validators, register them with dependency injection as scoped
            var validatorsInAssembly = AssemblyScanner.FindValidatorsInAssembly(typeof(AddSubscriberRequestValidator).Assembly);
            foreach (var validator in validatorsInAssembly)
            {
                builder.RegisterType(validator.ValidatorType).As(validator.InterfaceType);
            }

            builder.RegisterGeneric(typeof(ValidatorPipelineBehavior<,>)).As(typeof(IPipelineBehavior<,>));
        }
    }
}
