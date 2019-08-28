using Eshopworld.Core;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.Common.Telemetry;
using CaptainHook.Interfaces;

namespace CaptainHook.PoolManagerActor
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
    public class PoolManagerActor : Actor, IPoolManagerActor
    {
        private readonly IBigBrother _bigBrother;
        private readonly IActorProxyFactory _actorProxyFactory;
        private HashSet<int> _free; // free pool resources
        private Dictionary<Guid, MessageHook> _busy; // busy pool resources

        private const int NumberOfHandlers = 1000; // TODO: TWEAK THIS - HARDCODED FOR NOW

        /// <summary>
        /// Initializes a new instance of PoolManagerActor
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        /// <param name="bigBrother">The <see cref="IBigBrother"/> instanced used to publish telemetry.</param>
        /// <param name="actorProxyFactory"></param>
        public PoolManagerActor(ActorService actorService, ActorId actorId, IBigBrother bigBrother, IActorProxyFactory actorProxyFactory)
            : base(actorService, actorId)
        {
            _bigBrother = bigBrother;
            _actorProxyFactory = actorProxyFactory;
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override async Task OnActivateAsync()
        {
            _bigBrother.Publish(new ActorActivated(this));
            var free = await StateManager.TryGetStateAsync<HashSet<int>>(nameof(_free));

            if (free.HasValue)
            {
                //just a hack for now in case the sf is in a bad state with no free handlers and bad messages. A better solution is needed later
                if (free.Value.Count == default)
                {
                    Reset();
                }
                else
                {
                    _free = free.Value;
                    var busy = await StateManager.TryGetStateAsync<Dictionary<Guid, MessageHook>>(nameof(_busy));
                    if (busy.HasValue)
                    {
                        _busy = busy.Value;
                    }
                }
            }
            else
            {
                Reset();

                await StateManager.AddOrUpdateStateAsync(nameof(_free), _free, (s, value) => value);
                await StateManager.AddOrUpdateStateAsync(nameof(_busy), _busy, (s, value) => value);
            }

            _bigBrother.Publish(new PoolManagerActorTelemetryEvent("state of pool manager at activation time", this)
            {
                BusyHandlerCount = _busy.Count,
                FreeHandlerCount = _free.Count
            });
        }

        /// <summary>
        /// Rest the free and busy list to default values so msgs can be processed again
        /// </summary>
        private void Reset()
        {
            _busy = new Dictionary<Guid, MessageHook>(NumberOfHandlers);
            _free = Enumerable.Range(1, NumberOfHandlers).ToHashSet();
        }

        protected override async Task OnDeactivateAsync()
        {
            _bigBrother.Publish(new ActorDeactivated(this));

            await StateManager.AddOrUpdateStateAsync(nameof(_free), _free, (s, value) => value);
            await StateManager.AddOrUpdateStateAsync(nameof(_busy), _busy, (s, value) => value);

            _bigBrother.Publish(new PoolManagerActorTelemetryEvent("state of pool manager at deactivation time", this)
            {
                BusyHandlerCount = _busy.Count,
                FreeHandlerCount = _free.Count
            });
        }

        public async Task<Guid> DoWork(string payload, string type)
        {
            // need to handle the possibility of the resources in the pool being all busy!
            var handle = Guid.NewGuid();
            try
            {
                var handlerId = _free.FirstOrDefault();
                if (handlerId == default)
                {
                    throw new Exception("There are no free handlers in the pool manager to handle this event");
                }

                _free.Remove(handlerId);
                _busy.Add(handle, new MessageHook
                {
                    HandlerId = handlerId,
                    Type = type
                });

                await StateManager.AddOrUpdateStateAsync(nameof(_free), _free, (s, value) => value);
                await StateManager.AddOrUpdateStateAsync(nameof(_busy), _busy, (s, value) => value);

                await _actorProxyFactory.CreateActorProxy<IEventHandlerActor>(new ActorId(handlerId)).Handle(handle, payload, type);

                return handle;
            }
            catch (Exception e)
            {
                _bigBrother.Publish(e.ToExceptionEvent());
                await ReleaseHandle(handle);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="messageDelivered"></param>
        /// <returns></returns>
        public async Task CompleteWork(Guid handle, bool messageDelivered = true)
        {
            await ReleaseHandle(handle, messageDelivered);
        }

        /// <summary>
        /// Releases a handle from the pool manager state.
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="messageSuccess"></param>
        /// <returns></returns>
        private async Task ReleaseHandle(Guid handle, bool messageSuccess = false)
        {
            try
            {
                if (_busy.ContainsKey(handle))
                {
                    var msgHook = _busy[handle];
                    _busy.Remove(handle);
                    _free.Add(msgHook.HandlerId);

                    await StateManager.AddOrUpdateStateAsync(nameof(_free), _free, (s, value) => value);
                    await StateManager.AddOrUpdateStateAsync(nameof(_busy), _busy, (s, value) => value);

                    await _actorProxyFactory.CreateActorProxy<IEventReaderActor>(new ActorId(msgHook.Type)).CompleteMessage(handle, messageSuccess);
                }
                else
                {
                    _bigBrother.Publish(new PoolManagerActorTelemetryEvent($"Key {handle} not found in the dictionary", this)
                    {
                        BusyHandlerCount = _busy.Count,
                        FreeHandlerCount = _free.Count
                    });
                }
            }
            catch (Exception e)
            {
                _bigBrother.Publish(e.ToExceptionEvent());
                throw;
            }
        }
    }
}
