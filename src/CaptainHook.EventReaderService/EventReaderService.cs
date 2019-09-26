using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Telemetry.Service;
using CaptainHook.Interfaces;
using Eshopworld.Core;
using Eshopworld.Telemetry;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CaptainHook.EventReaderService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// Reads the messages from the subscription.
    /// Keeps an list of in flight messages and tokens which can be used for renewal as well as deleting the message from the subscription when it's complete
    /// </summary>
    public class EventReaderService : StatefulService, IEventReaderService
    {
        private const string SubscriptionName = "captain-hook";

        private const int BatchSize = 1; // make this dynamic - based on the number of active handlers - more handlers, lower batch

        private readonly IBigBrother _bigBrother;
        private readonly IServiceBusManager _serviceBusManager;
        private readonly IActorProxyFactory _proxyFactory;
        private readonly ConfigurationSettings _settings;
        private readonly string _eventType;

        private IReliableDictionary2<int, MessageDataHandle> _messageHandles;
        private ConcurrentHashSet<int> _freeHandlers = new ConcurrentHashSet<int>();
        private IMessageReceiver _messageReceiver;
        private CancellationToken _cancellationToken;

        //todo move this to config driven in the code package
        internal int HandlerCount = 10;
        private readonly TimeSpan _defaultServiceFabricStateOperationTimeout = TimeSpan.FromSeconds(4); // 4 seconds is default defined by state operation methods in service fabric docs

        /// <summary>
        /// Default ctor used at runtime
        /// </summary>
        /// <param name="context"></param>
        /// <param name="bigBrother"></param>
        /// <param name="serviceBusManager"></param>
        /// <param name="proxyFactory"></param>
        /// <param name="settings"></param>
        public EventReaderService(
            StatefulServiceContext context,
            IBigBrother bigBrother,
            IServiceBusManager serviceBusManager,
            IActorProxyFactory proxyFactory,
            ConfigurationSettings settings)
            : base(context)
        {
            _bigBrother = bigBrother;
            _serviceBusManager = serviceBusManager;
            _proxyFactory = proxyFactory;
            _settings = settings;
            _eventType = Encoding.UTF8.GetString(context.InitializationData);
        }

        /// <summary>
        /// Ctor used for mocking and tests
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reliableStateManagerReplica"></param>
        /// <param name="bigBrother"></param>
        /// <param name="serviceBusManager"></param>
        /// <param name="proxyFactory"></param>
        /// <param name="settings"></param>
        public EventReaderService(
            StatefulServiceContext context,
            IReliableStateManagerReplica reliableStateManagerReplica,
            IBigBrother bigBrother,
            IServiceBusManager serviceBusManager,
            IActorProxyFactory proxyFactory,
            ConfigurationSettings settings)
            : base(context, reliableStateManagerReplica)
        {
            _bigBrother = bigBrother;
            _serviceBusManager = serviceBusManager;
            _proxyFactory = proxyFactory;
            _settings = settings;
            _eventType = Encoding.UTF8.GetString(context.InitializationData);
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }

        protected override async Task OnOpenAsync(ReplicaOpenMode openMode, CancellationToken cancellationToken)
        {
            _bigBrother.Publish(new ServiceActivatedEvent(Context, await InFlightMessageCountAsync()));
            await base.OnOpenAsync(openMode, cancellationToken);
        }

        protected override async Task OnCloseAsync(CancellationToken cancellationToken)
        {
            _bigBrother.Publish(new ServiceDeactivatedEvent(Context, await InFlightMessageCountAsync()));
            await base.OnCloseAsync(cancellationToken);
        }

        protected override async void OnAbort()
        {
            _bigBrother.Publish(new ServiceAbortedEvent(Context, await InFlightMessageCountAsync()));
            base.OnAbort();
        }

        protected override async Task OnChangeRoleAsync(ReplicaRole newRole, CancellationToken cancellationToken)
        {
            _bigBrother.Publish(new ServiceRoleChangeEvent(Context, newRole, await InFlightMessageCountAsync()));
            await base.OnChangeRoleAsync(newRole, cancellationToken);
        }

        /// <summary>
        /// Determines the number of handlers to have based on the number of messages which are inflight
        /// </summary>
        /// <returns></returns>
        internal async Task GetHandlerCountsOnStartup()
        {
            using (var tx = StateManager.CreateTransaction())
            {
                var enumerator = (await _messageHandles.CreateKeyEnumerableAsync(tx)).GetAsyncEnumerator();

                var set = new HashSet<int>();
                while (await enumerator.MoveNextAsync(_cancellationToken))
                {
                    if (_cancellationToken.IsCancellationRequested) return;

                    var handleData = await _messageHandles.TryGetValueAsync(tx, enumerator.Current);
                    if (!handleData.HasValue) continue;

                    set.Add(handleData.Value.HandlerId);
                }

                await tx.CommitAsync();

                HandlerCount = Math.Max(HandlerCount, set.Count > 0 ? set.Max() : 0);
                _freeHandlers = Enumerable.Range(1, HandlerCount).Except(set.ToHashSet()).ToConcurrentHashSet();
            }
        }


        /// <summary>
        /// Gets the count of the numbers of messages which are in flight at the moment
        /// </summary>
        internal async Task<int> InFlightMessageCountAsync()
        {
            return  HandlerCount - await _freeHandlers.CountAsync(_cancellationToken);
        } 

        private async Task SetupServiceBus()
        {
            await _serviceBusManager.CreateAsync(_settings.AzureSubscriptionId, _settings.ServiceBusNamespace, SubscriptionName, _eventType);
            _messageReceiver = _serviceBusManager.CreateMessageReceiver(_settings.ServiceBusConnectionString, _eventType, SubscriptionName);
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;

            try
            {
                _messageHandles = await StateManager.GetOrAddAsync<IReliableDictionary2<int, MessageDataHandle>>(nameof(MessageDataHandle));

                await GetHandlerCountsOnStartup();
                await SetupServiceBus();

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        if (_messageReceiver.IsClosedOrClosing) continue;

                        var messages = await _messageReceiver.ReceiveAsync(BatchSize, TimeSpan.FromMilliseconds(50));
                        if (messages == null || messages.Count == 0)
                        {
                            // ReSharper disable once MethodSupportsCancellation - no need to cancellation token here
                            await Task.Delay(TimeSpan.FromMilliseconds(10));
                            continue;
                        }

                        foreach (var message in messages)
                        {
                            var messageData = new MessageData(Encoding.UTF8.GetString(message.Body), _eventType);

                            var handlerId = await GetFreeHandlerIdAsync();

                            messageData.HandlerId = handlerId;
                            messageData.CorrelationId = Guid.NewGuid().ToString();

                            var handleData = new MessageDataHandle
                            {
                                HandlerId = handlerId,
                                LockToken = _serviceBusManager.GetLockToken(message)
                            };

                            using (var tx = StateManager.CreateTransaction())
                            {
                                var result = await _messageHandles.TryAddAsync(tx, handleData.HandlerId, handleData, _defaultServiceFabricStateOperationTimeout, _cancellationToken);

                                if (!result)
                                {
                                    throw new FailureStateUpdateException(tx.TransactionId, handleData.HandlerId, _eventType, "Could not add message handle to state store in Reader Service", Context);
                                }

                                await _proxyFactory.CreateActorProxy<IEventHandlerActor>(
                                        new ActorId(messageData.EventHandlerActorId),
                                        serviceName: Constants.CaptainHookApplication.Services.EventHandlerServiceShortName)
                                    .Handle(messageData);

                                await tx.CommitAsync();
                            }
                        }
                    }
                    catch (ServiceBusCommunicationException sbCommunicationException)
                    {
                        BigBrother.Write(sbCommunicationException.ToExceptionEvent());
                        await SetupServiceBus();
                    }
                    catch (Exception e)
                    {
                        BigBrother.Write(e.ToExceptionEvent());
                    }
                }
            }
            catch (Exception e)
            {
                BigBrother.Write(e.ToExceptionEvent());
            }
        }

        internal async Task<int> GetFreeHandlerIdAsync()
        {
            var handlerId = await _freeHandlers.FirstOrDefaultAsync(_cancellationToken);
            if (handlerId == 0)
            {
                return ++HandlerCount;
            }

            await _freeHandlers.RemoveAsync(handlerId, _cancellationToken);
            return handlerId;
        }

        /// <summary>
        /// Completes the messages if the delivery was successful, else message is not removed from service bus and allowed to be redelivered
        /// </summary>
        /// <param name="messageData"></param>
        /// <param name="messageDelivered"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task CompleteMessageAsync(MessageData messageData, bool messageDelivered, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var tx = StateManager.CreateTransaction())
                {
                    var handle = await _messageHandles.TryRemoveAsync(tx, messageData.HandlerId, _defaultServiceFabricStateOperationTimeout, cancellationToken);
                    if (!handle.HasValue)
                    {
                        throw new LockTokenNotFoundException("lock token was not found in reliable state")
                        {
                            EventType = messageData.Type,
                            HandlerId = messageData.HandlerId,
                            CorrelationId = messageData.CorrelationId
                        };
                    }

                    try
                    {
                        //let the message naturally expire if it's an unsuccessful delivery
                        if (messageDelivered)
                        {
                            await _messageReceiver.CompleteAsync(handle.Value.LockToken);
                        }
                    }
                    catch (MessageLockLostException e)
                    {
                        BigBrother.Write(e.ToExceptionEvent());
                    }
                    finally
                    {
                        await tx.CommitAsync();
                    }
                }
            }
            catch (Exception e)
            {
                BigBrother.Write(e.ToExceptionEvent());
            }
            finally
            {
                await _freeHandlers.AddAsync(messageData.HandlerId, cancellationToken);
            }
        }
    }
}
