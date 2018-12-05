namespace CaptainHook.PoolManagerActor
{
    using Common;
    using Eshopworld.Core;
    using Interfaces;
    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Actors.Client;
    using Microsoft.ServiceFabric.Actors.Runtime;
    using Microsoft.ServiceFabric.Data;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    public class PoolManagerActor : Actor, IPoolManagerActor
    {
        private readonly IBigBrother _bigBrother;
        private ConditionalValue<HashSet<int>> _free; // free pool resources
        private ConditionalValue<Dictionary<Guid, MessageHook>> _busy; // busy pool resources

        private const int NumberOfHandlers = 10; // TODO: TWEAK THIS - HARDCODED FOR NOW

        /// <summary>
        /// Initializes a new instance of PoolManagerActor
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        /// <param name="bigBrother">The <see cref="IBigBrother"/> instanced used to publish telemetry.</param>
        public PoolManagerActor(ActorService actorService, ActorId actorId, IBigBrother bigBrother)
            : base(actorService, actorId)
        {
            _bigBrother = bigBrother;
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override async Task OnActivateAsync()
        {
            _free = await StateManager.TryGetStateAsync<HashSet<int>>("free");

            if (_free.HasValue)
            {
                _busy = await StateManager.TryGetStateAsync<Dictionary<Guid, MessageHook>>(nameof(_busy));
            }
            else
            {
                _free = new ConditionalValue<HashSet<int>>(true, new HashSet<int>());
                _busy = new ConditionalValue<Dictionary<Guid, MessageHook>>(true, new Dictionary<Guid, MessageHook>());

                for (var i = 0; i < NumberOfHandlers; i++)
                {
                    ActorProxy.Create<IEventHandlerActor>(new ActorId(i)); // TODO: this probably isn't needed here since we're not invoking the actor at this point - REVIEW
                    _free.Value.Add(i);
                }

                await StateManager.AddOrUpdateStateAsync(nameof(_free), _free, (s, value) => value);
                await StateManager.AddOrUpdateStateAsync(nameof(_busy), _busy, (s, value) => value);
            }
        }

        public async Task<Guid> DoWork(string payload, string type)
        {
            // need to handle the possibility of the resources in the pool being all busy!

            try
            {
                var handlerId = _free.Value.First();
                var handle = Guid.NewGuid();
                _free.Value.Remove(handlerId);
                _busy.Value.Add(
                    handle,
                    new MessageHook
                    {
                        HandlerId = handlerId,
                        Type = type
                    });

                await StateManager.AddOrUpdateStateAsync(nameof(_free), _free, (s, value) => value);
                await StateManager.AddOrUpdateStateAsync(nameof(_busy), _busy, (s, value) => value);

                await ActorProxy.Create<IEventHandlerActor>(new ActorId(handlerId)).Handle(handle, payload, type);

                return handle;
            }
            catch (Exception e)
            {
                _bigBrother.Publish(e.ToExceptionEvent());
                throw;
            }
        }

        public async Task CompleteWork(Guid handle)
        {
            try
            {
                var msgHook = _busy.Value[handle];
                _busy.Value.Remove(handle);
                _free.Value.Add(msgHook.HandlerId);

                await StateManager.AddOrUpdateStateAsync(nameof(_free), _free, (s, value) => value);
                await StateManager.AddOrUpdateStateAsync(nameof(_busy), _busy, (s, value) => value);

                await ActorProxy.Create<IEventReaderActor>(new ActorId(msgHook.Type)).CompleteMessage(handle);
            }
            catch (Exception e)
            {
                _bigBrother.Publish(e.ToExceptionEvent());
                throw;
            }
        }
    }
}
