using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        private readonly CancellationTokenSource _cancellationTokenSource;

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
            _cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override async Task OnActivateAsync()
        {
            _bigBrother.Publish(new ActorActivated(this));

            var names = (await StateManager.GetStateNamesAsync()).Take(1).ToList();
            if (names.Any())
            {
                _handleTimer = RegisterTimer(
                    InternalHandle,
                    names.FirstOrDefault(),
                    TimeSpan.FromMilliseconds(100),
                    TimeSpan.MaxValue);
            }
        }

        protected override Task OnDeactivateAsync()
        {
            if (_handleTimer != null)
            {
                UnregisterTimer(_handleTimer);
            }
            _cancellationTokenSource.Cancel();

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

            await StateManager.AddOrUpdateStateAsync(handle.ToString(), messageData, (s, pair) => pair);

            _handleTimer = RegisterTimer(
                InternalHandle,
                handle.ToString(),
                TimeSpan.FromMilliseconds(100),
                TimeSpan.MaxValue);
        }

        private async Task InternalHandle(object state)
        {
            var correlationId = Guid.NewGuid();
            var handle = Guid.NewGuid();
            try
            {
                UnregisterTimer(_handleTimer);

                if (state != null)
                {
                    var result = Guid.TryParse(state.ToString(), out handle);
                    if (!result)
                    {
                        _bigBrother.Publish(new ActorError($"{state} could not be parsed to a guid so removing it.", this));
                        return;
                    }
                }
                else
                {
                    _bigBrother.Publish(new ActorError($"Timer state was null so cannot process any message", this));
                    return;
                }

                var messageDataConditional = await StateManager.TryGetStateAsync<MessageData>(handle.ToString());
                if (!messageDataConditional.HasValue)
                {
                    _bigBrother.Publish(new ActorError("message from state manager was empty", this));
                    return;
                }

                var messageData = messageDataConditional.Value;
                handle = messageData.Handle;
                messageData.CorrelationId = correlationId.ToString();

                var handler = _eventHandlerFactory.CreateEventHandler(messageData.Type);

                await handler.CallAsync(messageData, new Dictionary<string, object>(), _cancellationTokenSource.Token);
            }
            catch (Exception e)
            {
                //don't want msg state managed by fabric just yet, let failures be backed by the service bus subscriptions
                BigBrother.Write(e.ToExceptionEvent());
            }
            finally
            {
                await StateManager.RemoveStateAsync(handle.ToString());
                await ActorProxy.Create<IPoolManagerActor>(new ActorId(0)).CompleteWork(handle);
            }
        }
    }
}
