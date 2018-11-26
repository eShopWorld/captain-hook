namespace CaptainHook.EventReaderActor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Common;
    using Common.Telemetry;
    using Eshopworld.Core;
    using Interfaces;
    using Microsoft.Azure.Management.Fluent;
    using Microsoft.Azure.Management.ResourceManager.Fluent;
    using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
    using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Core;
    using Microsoft.Azure.Services.AppAuthentication;
    using Microsoft.Rest;
    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Actors.Client;
    using Microsoft.ServiceFabric.Actors.Runtime;
    using Microsoft.ServiceFabric.Data;

    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    public class EventReaderActor : Actor, IEventReaderActor
    {
        private const string SubscriptionName = "captain-hook";

        // TAKE NUMBER OF HANDLERS INTO CONSIDERATION, DO NOT BATCH MORE THEN HANDLERS
        private const int BatchSize = 10; // make this configurable

        private readonly IBigBrother _bb;
        private readonly ConfigurationSettings _settings;
        private IActorTimer _poolTimer;
        private MessageReceiver _receiver;

        private ConditionalValue<Dictionary<Guid, string>> _messagesInHandlers;

        /// <summary>
        /// Initializes a new instance of EventReaderActor
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        /// <param name="bb">The <see cref="IBigBrother"/> telemetry instance that this actor instance will use to publish.</param>
        /// <param name="settings">The <see cref="ConfigurationSettings"/> being read from the KeyVault.</param>
        public EventReaderActor(ActorService actorService, ActorId actorId, IBigBrother bb, ConfigurationSettings settings)
            : base(actorService, actorId)
        {
            _bb = bb;
            _settings = settings;
        }

        protected override async Task OnActivateAsync()
        {
            _bb.Publish(new ActorActivated(this));

            var inHandlers = await StateManager.TryGetStateAsync<Dictionary<Guid, string>>(nameof(_messagesInHandlers));
            if (inHandlers.HasValue)
            {
                _messagesInHandlers = inHandlers;
            }
            else
            {
                _messagesInHandlers = new ConditionalValue<Dictionary<Guid, string>>(true, new Dictionary<Guid, string>());
                await StateManager.AddOrUpdateStateAsync(nameof(_messagesInHandlers), _messagesInHandlers, (s, value) => _messagesInHandlers);
            }

            //todo will need more namespace setup per tenant
            await SetupServiceBus();

            _poolTimer = RegisterTimer(
                ReadEvents,
                null,
                TimeSpan.FromMilliseconds(100),
                TimeSpan.FromMilliseconds(100));

            _receiver = new MessageReceiver(
                _settings.ServiceBusConnectionString,
                EntityNameHelper.FormatSubscriptionPath(TypeExtensions.GetEntityName(Id.GetStringId()), SubscriptionName),
                ReceiveMode.PeekLock,
                new RetryExponential(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(500), 3),
                BatchSize);

            await base.OnActivateAsync();
        }

        protected override Task OnDeactivateAsync()
        {
            if (_poolTimer != null)
            {
                UnregisterTimer(_poolTimer);
            }

            return base.OnDeactivateAsync();
        }

        internal async Task SetupServiceBus()
        {
            var token = new AzureServiceTokenProvider().GetAccessTokenAsync("https://management.core.windows.net/", string.Empty).Result;
            var tokenCredentials = new TokenCredentials(token);

            var client = RestClient.Configure()
                                   .WithEnvironment(AzureEnvironment.AzureGlobalCloud)
                                   .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                                   .WithCredentials(new AzureCredentials(tokenCredentials, tokenCredentials, string.Empty, AzureEnvironment.AzureGlobalCloud))
                                   .Build();

            var sbNamespace = Azure.Authenticate(client, string.Empty)
                                   .WithSubscription(_settings.AzureSubscriptionId)
                                   .ServiceBusNamespaces.List()
                                   .SingleOrDefault(n => n.Name == _settings.ServiceBusNamespace);

            if (sbNamespace == null)
            {
                throw new InvalidOperationException($"Couldn't find the service bus namespace {_settings.ServiceBusNamespace} in the subscription with ID {_settings.AzureSubscriptionId}");
            }

            var azureTopic = await sbNamespace.CreateTopicIfNotExists(TypeExtensions.GetEntityName(Id.GetStringId()));
            await azureTopic.CreateSubscriptionIfNotExists(SubscriptionName);
        }

        internal async Task ReadEvents(object _)
        {
            if (_receiver.IsClosedOrClosing) return;

            var messages = await _receiver.ReceiveAsync(BatchSize).ConfigureAwait(false);
            if (messages == null) return;

            foreach (var message in messages)
            {
                var handle = await ActorProxy.Create<IPoolManagerActor>(new ActorId(0)).DoWork(Encoding.UTF8.GetString(message.Body), Id.GetStringId());
                _messagesInHandlers.Value.Add(handle, message.SystemProperties.LockToken);
                await StateManager.AddOrUpdateStateAsync(nameof(_messagesInHandlers), _messagesInHandlers, (s, value) => _messagesInHandlers);
            }
        }

        /// <remarks>
        /// Do nothing by design. We just need to make sure that the actor is properly activated.
        /// </remarks>>
        public async Task Run()
        {
            await Task.Yield();
        }

        public async Task CompleteMessage(Guid handle)
        {
            // NOT HANDLING FAULTS YET - BE CAREFUL HERE!

            await _receiver.CompleteAsync(_messagesInHandlers.Value[handle]);
            _messagesInHandlers.Value.Remove(handle);
            await StateManager.AddOrUpdateStateAsync(nameof(_messagesInHandlers), _messagesInHandlers, (s, value) => _messagesInHandlers);
        }
    }
}
