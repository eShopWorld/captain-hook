using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.Common.Telemetry.Actor;
using CaptainHook.EventHandlerActor.Handlers;
using CaptainHook.Interfaces;
using Eshopworld.Core;
using Eshopworld.Telemetry;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;

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
            _bigBrother.Publish(new ActorDeactivated(this));
            return base.OnDeactivateAsync();
        }

        public async Task Handle(MessageData messageData)
        {
            _bigBrother.Publish(new ActorHandleStarted(messageData, this));

            await StateManager.AddOrUpdateStateAsync(messageData.HandlerId.ToString(), messageData, (s, pair) => pair);

            _handleTimer = RegisterTimer(
                InternalHandle,
                messageData,
                TimeSpan.FromMilliseconds(100),
                TimeSpan.MaxValue);
        }

        private async Task InternalHandle(object state)
        {
            var messageDelivered = true;
            MessageData messageData = null;
            try
            {
                UnregisterTimer(_handleTimer);

                messageData = state as MessageData;
                if (messageData == null)
                {
                    _bigBrother.Publish(new ActorStateError(state, this));
                    return;
                }

                if (string.IsNullOrWhiteSpace(messageData.Type))
                {
                    _bigBrother.Publish(new ActorError($"message is missing type - it cannot be processed", this));
                    return;
                }

                var handler = _eventHandlerFactory.CreateEventHandler(messageData);
                messageDelivered = await handler.CallAsync(messageData, new Dictionary<string, object>(), CancellationToken.None);
            }
            catch (Exception e)
            {
                BigBrother.Write(e.ToExceptionEvent());
                messageDelivered = false;
            }
            finally
            {
                if (messageData != null)
                {

                    try
                    {
                        await StateManager.RemoveStateAsync(messageData.HandlerId.ToString());

                        await ServiceProxy.Create<IEventReaderService>(new Uri(messageData.ReplyTo)).CompleteMessageAsync(messageData, messageDelivered);

                    }
                    catch (Exception e)
                    {
                        BigBrother.Write(e.ToExceptionEvent());
                    }
                }
            }
        }
    }
}
