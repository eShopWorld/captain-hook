namespace CaptainHook.EventHandlerActor
{
    using System;
    using System.Threading.Tasks;
    using Common.Telemetry;
    using Eshopworld.Core;
    using Eshopworld.Telemetry;
    using Handlers;
    using Interfaces;
    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Actors.Client;
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
                // Assuming whatever I'm calling can handle idempotency

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

            await StateManager.AddOrUpdateStateAsync(nameof(EventHandlerActor), messageData, (s, pair) => pair);

            _handleTimer = RegisterTimer(
                InternalHandle,
                null,
                TimeSpan.FromMilliseconds(100),
                TimeSpan.MaxValue);
        }

        /// <remarks>
        /// Not used in this case, because we are hard-coding all handling logic in this Actor, so there's no need to handle completion in higher actors.
        /// </remarks>>
        public async Task CompleteHandle(Guid handle)
        {
            await Task.Yield();
            throw new NotImplementedException("Not used - nothing above this actor will actually be called in v0");
        }

        private async Task InternalHandle(object _)
        {
            UnregisterTimer(_handleTimer);

            var messageData = await StateManager.TryGetStateAsync<MessageData>(nameof(EventHandlerActor));
            if (!messageData.HasValue)
            {
               _bigBrother.Publish(new WebhookEvent("message was empty") );
                return;
            }

            try
            {
                //todo nuke this in V1
                var (brandType, domainType) = ModelParser.ParseBrandAndDomainType(messageData.Value.Payload);

                //todo need to register the handlers based on the contents of the domain events and the data in the messages
                var handler = _handlerFactory.CreateHandler(brandType, domainType);

                await handler.Call(messageData.Value);

                await StateManager.RemoveStateAsync(nameof(EventHandlerActor));
                await ActorProxy.Create<IPoolManagerActor>(new ActorId(0)).CompleteWork(messageData.Value.Handle);
            }
            catch (Exception e)
            {
                BigBrother.Write(e);
            }
        }
    }
}
