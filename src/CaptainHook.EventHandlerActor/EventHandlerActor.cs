using System;
using System.Linq;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.Common.Telemetry;
using CaptainHook.EventHandlerActor.Handlers;
using CaptainHook.Interfaces;
using Eshopworld.Core;
using Eshopworld.Telemetry;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace CaptainHook.EventHandlerActor
{
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
        private readonly IEventHandlerFactory _eventHandlerFactory;
        private readonly IBigBrother _bigBrother;
        private IActorTimer _handleTimer;

        /// <summary>
        /// Initializes a new instance of EventHandlerActor
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        /// <param name="eventHandlerFactory"></param>
        /// <param name="bigBrother"></param>
        public EventHandlerActor(
            ActorService actorService,
            ActorId actorId,
            IEventHandlerFactory eventHandlerFactory,
            IBigBrother bigBrother)
            : base(actorService, actorId)
        {
            _eventHandlerFactory = eventHandlerFactory;
            _bigBrother = bigBrother;
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override async Task OnActivateAsync()
        {
            _bigBrother.Publish(new ActorActivated(this));

            var names = await StateManager.GetStateNamesAsync();
            if (names.Any())
            {
                _handleTimer = RegisterTimer(
                    InternalHandle,
                    null,
                    TimeSpan.FromMilliseconds(100),
                    TimeSpan.MaxValue);
            }
        }

        protected override Task OnDeactivateAsync()
        {
            _bigBrother.Publish(new ActorDeactivated(this));
            return base.OnDeactivateAsync();
        }

        public async Task Handle(Guid handle, string payload, string type)
        {
            var messageData = new MessageData
            {
                Handle = handle,
                Payload = payload,
                Type = type
            };

            await StateManager.AddOrUpdateStateAsync(messageData.Handle.ToString(), messageData, (s, pair) => pair);

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
            var handle = string.Empty;
            try
            {
                UnregisterTimer(_handleTimer);

                var handleList = (await StateManager.GetStateNamesAsync()).ToList();

                if (!handleList.Any())
                {
                    return;
                }

                var messageDataConditional = await StateManager.TryGetStateAsync<MessageData>(handleList.First());
                if (!messageDataConditional.HasValue)
                {
                    _bigBrother.Publish(new WebhookEvent("message was empty"));
                    return;
                }

                var messageData = messageDataConditional.Value;
                handle = messageData.Handle.ToString();

                var handler = _eventHandlerFactory.CreateEventHandler(messageData.Type);

                await handler.Call(messageData);

                await StateManager.RemoveStateAsync(messageData.Handle.ToString());
                await ActorProxy.Create<IPoolManagerActor>(new ActorId(0)).CompleteWork(messageData.Handle);
            }
            catch (Exception e)
            {
                //don't want msg state managed by fabric just yet, let failures be backed by the service bus subscriptions
                if (handle != string.Empty)
                {
                    await StateManager.RemoveStateAsync(handle);
                }

                BigBrother.Write(e.ToExceptionEvent());
            }
            finally
            {
                //restarts the timer in case there are more than one msg in the state, if not then let it be restarted in the standard msg population flow.
                if ((await StateManager.GetStateNamesAsync()).Any())
                {
                    _handleTimer = RegisterTimer(
                        InternalHandle,
                        null,
                        TimeSpan.FromMilliseconds(100),
                        TimeSpan.MaxValue);
                }
            }
        }
    }
}
