using Autofac;
using CaptainHook.Common.Configuration;
using CaptainHook.EventHandlerActor.Handlers;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using CaptainHook.EventHandlerActor.Handlers.Requests;

namespace CaptainHook.EventHandlerActor
{
    public class HandlerModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DefaultRequestBuilder>().Keyed<IRequestBuilder>(RuleAction.Route);
            builder.RegisterType<RouteAndReplaceRequestBuilder>().Keyed<IRequestBuilder>(RuleAction.RouteAndReplace);
            builder.RegisterType<RequestBuilderDispatcher>().As<IRequestBuilder>();

            builder.RegisterType<EventHandlerFactory>().As<IEventHandlerFactory>().SingleInstance();
            builder.RegisterType<AuthenticationHandlerFactory>().As<IAuthenticationHandlerFactory>().SingleInstance();
            builder.RegisterType<HttpClientFactory>().As<IHttpClientFactory>();
            builder.RegisterType<RequestLogger>().As<IRequestLogger>();
        }
    }
}