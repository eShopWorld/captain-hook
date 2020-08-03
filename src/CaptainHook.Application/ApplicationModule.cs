using System;
using Autofac;
using CaptainHook.Application.Handlers.Subscribers;
using CaptainHook.Application.Infrastructure;
using CaptainHook.Application.Infrastructure.DirectorService;
using CaptainHook.Application.Infrastructure.DirectorService.Remoting;
using CaptainHook.Application.Infrastructure.Mappers;
using CaptainHook.Application.Validators;
using CaptainHook.Common;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace CaptainHook.Application
{
    public class ApplicationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var directorServiceUri = new Uri(ServiceNaming.DirectorServiceFullName);
            var directorServiceClient = ServiceProxy.Create<IDirectorServiceRemoting>(directorServiceUri);
            builder.RegisterInstance(directorServiceClient).As<IDirectorServiceRemoting>().SingleInstance();

            builder.RegisterType<DirectorServiceProxy>().As<IDirectorServiceProxy>();
            builder.RegisterType<SubscriberEntityToConfigurationMapper>().As<ISubscriberEntityToConfigurationMapper>();

            var handlersAssembly = typeof(UpsertWebhookRequestHandler).Assembly;
            var validatorsAssembly = typeof(UpsertWebhookRequestValidator).Assembly;
            builder.RegisterMediatorInfrastructure(handlersAssembly, validatorsAssembly);
        }
    }
}
