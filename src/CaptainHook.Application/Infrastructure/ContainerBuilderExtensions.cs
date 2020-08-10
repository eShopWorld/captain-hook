using System.Reflection;
using Autofac;
using FluentValidation;
using MediatR;

namespace CaptainHook.Application.Infrastructure
{
    public static class ContainerBuilderExtensions
    {
        public static ContainerBuilder RegisterMediatorInfrastructure(this ContainerBuilder builder, Assembly handlersAssembly)
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
                .RegisterAssemblyTypes(handlersAssembly)
                .AsClosedTypesOf(typeof(IRequestHandler<,>))
                .AsImplementedInterfaces();

            builder.RegisterGeneric(typeof(ValidatorPipelineBehavior<,>)).As(typeof(IPipelineBehavior<,>));

            return builder;
        }

        public static ContainerBuilder RegisterValidationInfrastructure(this ContainerBuilder builder, Assembly assembly)
        {
            var validatorsInAssembly = AssemblyScanner.FindValidatorsInAssembly(assembly);
            foreach (var validator in validatorsInAssembly)
            {
                builder.RegisterType(validator.ValidatorType).As(validator.InterfaceType);
            }

            return builder;
        }
    }
}