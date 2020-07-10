using Autofac;
using CaptainHook.Domain.Handlers.Subscribers;
using CaptainHook.Domain.RequestValidators;
using FluentValidation;
using MediatR;
using System.Reflection;
using CaptainHook.Domain.Infrastructure;

namespace CaptainHook.Domain
{
    public class DomainModule : Autofac.Module
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
