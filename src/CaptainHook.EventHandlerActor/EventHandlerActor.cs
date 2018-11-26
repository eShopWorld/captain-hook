namespace CaptainHook.EventHandlerActor
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Autofac.Features.Indexed;
    using Common.Authentication;
    using Common.Telemetry;
    using Eshopworld.Core;
    using Interfaces;
    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Actors.Runtime;

    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    public class EventHandlerActor : Actor, IEventHandlerActor
    {
        private readonly IHandlerFactory _handlerFactory;
        private readonly IBigBrother _bigBrother;
        private IActorTimer _handleTimer;

        /// <summary>
        /// Initializes a new instance of EventHandlerActor
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        /// <param name="handlerFactory"></param>
        /// <param name="bigBrother"></param>
        public EventHandlerActor(
            ActorService actorService,
            ActorId actorId,
            IHandlerFactory handlerFactory,
            IBigBrother bigBrother)
            : base(actorService, actorId)
        {
            _handlerFactory = handlerFactory;
            _bigBrother = bigBrother;
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override async Task OnActivateAsync()
        {
            if ((await StateManager.TryGetStateAsync<MessageData>(nameof(EventHandlerActor))).HasValue)
            {
                // There's a message to handle, but we're not sure if it was fully handled or not, so we are going to handle it anyways

                _handleTimer = RegisterTimer(
                    InternalHandle,
                    null,
                    TimeSpan.FromMilliseconds(100),
                    TimeSpan.MaxValue);
            }
        }

        public async Task Handle(Guid handle, string payload, string type)
        {
            var messageData = new MessageData
            {
                Handle = handle,
                Payload = payload,
                Type = type
            };

            await StateManager.AddOrUpdateStateAsync(nameof(EventHandlerActor), messageData, (s, pair) => messageData);

            _handleTimer = RegisterTimer(
                InternalHandle,
                null,
                TimeSpan.FromMilliseconds(100),
                TimeSpan.MaxValue);
        }

        public async Task CompleteHandle(Guid handle)
        {
            await Task.Yield();
            throw new NotImplementedException();
        }

        private async Task InternalHandle(object _)
        {
            UnregisterTimer(_handleTimer);

            var messageData = await StateManager.TryGetStateAsync<MessageData>(nameof(EventHandlerActor));
            if (!messageData.HasValue)
            {
                //todo event on this flow.
                return;
            }

            // TODO: HANDLE THE THING - PROBABLY PUT A TRANSACTION HERE AND SCOPE IT TO THE STATEMANAGER CALL

            //brandtype v message type
            var handler = _handlerFactory.CreateHandler(messageData.Value.Type);

            if (handler != null)
            {
                await handler.MakeCall(messageData.Value);
                _bigBrother.Publish(new MessageExecuted
                {
                    Payload = messageData.Value.Payload,
                    Type = messageData.Value.Type
                });
            }
            else
            {
                _bigBrother.Publish(new UnknownMessageType
                {
                    Payload = messageData.Value.Payload,
                    Type = messageData.Value.Type
                });
            }
            await StateManager.RemoveStateAsync(nameof(EventHandlerActor));
        }
    }

    public interface IHandlerFactory
    {
        IHandler CreateHandler(string brandType);
    }

    public class HandlerFactory : IHandlerFactory
    {
        private readonly HttpClient _client;
        private readonly AuthConfig _authConfig;
        private readonly IIndex<string, WebHookConfig> _webHookConfig;

        public HandlerFactory(
            HttpClient client,
            AuthConfig authConfig,
            IIndex<string, WebHookConfig> webHookConfig)
        {
            _client = client;
            _authConfig = authConfig;
            _webHookConfig = webHookConfig;
        }

        public IHandler CreateHandler(string brandType)
        {
            switch (brandType)
            {
                case "MAX":
                    {
                        var handler = new GenericEventHandler(_authConfig, _webHookConfig["MAX"]);
                        return handler;
                    }
                case "DIF":
                    {
                        var handler = new GenericEventHandler(_authConfig, _webHookConfig["DIF"]);
                        return handler;
                    }
                case "GOC":
                    {
                        var handler = new GenericEventHandler(_authConfig, _webHookConfig["GOC"]);
                        return handler;
                    }

                default:
                    //todo this should not happen but should defend against it
                    break;
            }

            return null;
        }
    }
}
