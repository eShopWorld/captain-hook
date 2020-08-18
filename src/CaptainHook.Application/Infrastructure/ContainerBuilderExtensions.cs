using System.Reflection;
using Autofac;
using FluentValidation;
using MediatR;

namespace CaptainHook.Application.Infrastructure
{
    public static class ContainerBuilderExtensions
    {
        public static void RegisterMediatorInfrastructure(this ContainerBuilder builder, Assembly handlersAssembly, Assembly validatorsAssembly)
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

            var validatorsInAssembly = AssemblyScanner.FindValidatorsInAssembly(validatorsAssembly);
            foreach (var validator in validatorsInAssembly)
            {
                builder.RegisterType(validator.ValidatorType).As(validator.InterfaceType);
            }

            builder.RegisterGeneric(typeof(ExceptionHandlingPipelineBehavior<,>)).As(typeof(IPipelineBehavior<,>));
            builder.RegisterGeneric(typeof(ValidatorPipelineBehavior<,>)).As(typeof(IPipelineBehavior<,>));
        }
    }
}