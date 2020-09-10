using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.ServiceBus;
using CaptainHook.Common.ServiceModels;
using CaptainHook.Interfaces;
using Eshopworld.Core;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Moq;
using Newtonsoft.Json;
using ServiceFabric.Mocks;
using ServiceFabric.Mocks.ReplicaSet;
using Xunit;

namespace CaptainHook.Tests.Services.Reliable
{
    public class EventReaderServiceTests
    {
        private readonly StatefulServiceContext _context;

        private readonly IReliableStateManagerReplica2 _stateManager;

        private readonly IBigBrother _mockedBigBrother;

        private readonly ConfigurationSettings _config;

        private readonly MockActorProxyFactory _mockActorProxyFactory;

        private readonly Mock<IMessageReceiver> _mockMessageProvider;

        public EventReaderServiceTests()
        {
            var subscriberConfiguration = new SubscriberConfiguration
            {
                SubscriberName = "subA",
                EventType = "test.type",
            };

            _context = CustomMockStatefulServiceContextFactory.Create(
                ServiceNaming.EventReaderServiceType,
                ServiceNaming.EventReaderServiceFullUri("test.type", "subA"),
                EventReaderInitData.FromSubscriberConfiguration(subscriberConfiguration).ToByteArray(),
                replicaId: (new Random(int.MaxValue)).Next());
            _mockActorProxyFactory = new MockActorProxyFactory();
            _stateManager = new MockReliableStateManager();
            _config = new ConfigurationSettings();
            _mockedBigBrother = new Mock<IBigBrother>().Object;
            _mockMessageProvider = new Mock<IMessageReceiver>();
        }

        private Mock<IServiceBusManager> CreateMockServiceBusManager(string eventName = "", int messageCount = 0)
        {
            var count = 0;
            _mockMessageProvider
                .Setup(s => s.ReceiveAsync(It.IsAny<int>(), It.IsAny<TimeSpan>()))
                .ReturnsAsync(() =>
                {
                    if (count >= messageCount)
                    {
                        return new List<Message>();
                    }

                    count++;
                    return CreateMessage(eventName);
                });


            var mockServiceBusManager = new Mock<IServiceBusManager>();
            mockServiceBusManager
                .Setup(s => s.CreateSubscriptionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()));
            mockServiceBusManager
                .Setup(s => s.CreateMessageReceiver(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(_mockMessageProvider.Object);
            mockServiceBusManager
                .Setup(s => s.GetLockToken(It.IsAny<Message>()))
                .Returns(Guid.NewGuid().ToString);

            return mockServiceBusManager;
        }

        private EventReaderService.EventReaderService CreateService(
            Mock<IServiceBusManager> mockServiceBusManager, 
            MockActorProxyFactory mockActorProxyFactory = null, 
            StatefulServiceContext context = null)
        {
            return new EventReaderService.EventReaderService(
                context ?? _context,
                _stateManager,
                _mockedBigBrother,
                mockServiceBusManager.Object,
                mockActorProxyFactory ?? _mockActorProxyFactory,
                _config);
        }

        /// <summary>
        /// Tests the reader can get a message from ServiceBus and create a handler to process it.
        /// Expectation is that state in the reader will contain handlers in the reliable dictionary
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="handlerName"></param>
        /// <param name="expectedHandleCount"></param>
        /// <param name="messageCount">limit to simulate the number of messages in the queue in the mock</param>
        /// <returns></returns>
        [Theory]
        [IsUnit]
        [InlineData("test.type", "test.type-1", 1, 1)]
        public async Task CanGetMessage(string eventName, string handlerName, int expectedHandleCount, int messageCount)
        {
            _mockActorProxyFactory.RegisterActor(CreateMockEventHandlerActor(new ActorId(handlerName)));
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            var mockServiceBusManager = CreateMockServiceBusManager(eventName, messageCount);
            var service = CreateService(mockServiceBusManager);

            await service.InvokeOnOpenAsync(ReplicaOpenMode.New, cancellationTokenSource.Token);
            await service.InvokeRunAsync(cancellationTokenSource.Token);

            //Assert that the dictionary contains 1 processing message and associated handle            
            expectedHandleCount.Should().Be(service._inflightMessages.Count);
        }

        [Fact]
        [IsUnit]
        public async Task CanCancelService()
        {
            var mockServiceBusManager = CreateMockServiceBusManager();
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            var service = CreateService(mockServiceBusManager, new MockActorProxyFactory());

            await service.InvokeOnOpenAsync(ReplicaOpenMode.New, cancellationTokenSource.Token);
            await service.InvokeRunAsync(cancellationTokenSource.Token);

            //Assert can cancel the service from running
            cancellationTokenSource.IsCancellationRequested.Should().BeTrue();
        }

        [Theory]
        [IsUnit]
        [InlineData("test.type-subA", 3, 10)]
        [InlineData("test.type-subA", 10, 10)]
        [InlineData("test.type-subA", 12, 12)]
        public async Task CanAddMoreHandlersDynamically(string eventName, int messageCount, int expectedHandlerId)
        {
            //create mocked handlers based on the amount of messages passed in to the test
            var mockActorProxyFactory = new MockActorProxyFactory();
            for (var i = 0; i <= messageCount; i++)
            {
                mockActorProxyFactory.RegisterActor(CreateMockEventHandlerActor(new ActorId($"{eventName}-{i + 1}")));
            }

            var mockServiceBusManager = CreateMockServiceBusManager(eventName, messageCount);
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var service = CreateService(mockServiceBusManager, mockActorProxyFactory);

            await service.InvokeOnOpenAsync(ReplicaOpenMode.New, cancellationTokenSource.Token);
            await service.InvokeRunAsync(cancellationTokenSource.Token);

            expectedHandlerId.Should().Be(service.HandlerCount);
        }

        [Theory]
        [IsUnit]
        [InlineData("test.type", "test.type-1", 1, 1, true, 0)]
        [InlineData("test.type", "test.type-1", 1, 1, false, 0)]
        public async Task CanDeleteMessageFromSubscription(
            string eventName,
            string handlerName,
            int messageCount,
            int expectedHandlerId,
            bool messageDelivered,
            int expectedStatMessageCount)
        {
            _mockActorProxyFactory.RegisterActor(CreateMockEventHandlerActor(new ActorId(handlerName)));
            var mockServiceBusManager = CreateMockServiceBusManager(eventName, messageCount);
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            var service = CreateService(mockServiceBusManager);

            await service.InvokeOnOpenAsync(ReplicaOpenMode.New, cancellationTokenSource.Token);
            await service.InvokeRunAsync(cancellationTokenSource.Token);

            var dictionary = await _stateManager.GetOrAddAsync<IReliableDictionary2<int, MessageDataHandle>>(nameof(MessageDataHandle));

            MessageData messageData;
            using (var tx = _stateManager.CreateTransaction())
            {
                var messageDataHandle = await dictionary.TryGetValueAsync(tx, expectedHandlerId);
                //reconstruct the message so we can call complete
                messageData = new MessageData("Hello World 1", eventName, "subA", "service");
            }

            await service.CompleteMessageAsync(messageData, messageDelivered, CancellationToken.None);

            //Assert that the dictionary contains 1 processing message and associated handle
            dictionary = await _stateManager.GetOrAddAsync<IReliableDictionary2<int, MessageDataHandle>>(nameof(MessageDataHandle));
            Assert.Equal(expectedStatMessageCount, dictionary.Count);
        }

        [Theory]
        [IsUnit]
        [InlineData("test.type", "test.type-subA-1", 1)]
        [InlineData("test.type", "test.type-subA-2", 5)]
        public async Task InitSubscriberDataIsPassedToHandlers(string eventName, string handlerName, int messageCount)
        {
            var actor = CreateMockEventHandlerActor(new ActorId(handlerName));
            _mockActorProxyFactory.RegisterActor(actor);

            var subscriberConfiguration = new SubscriberConfiguration { EventType = eventName, SubscriberName = "subA" };

            var mockServiceBusManager = CreateMockServiceBusManager(eventName, messageCount);
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            var service = CreateService(mockServiceBusManager);

            await service.InvokeOnOpenAsync(ReplicaOpenMode.New, cancellationTokenSource.Token);
            await service.InvokeRunAsync(cancellationTokenSource.Token);

            var subscriberConfigurations = actor.MessageDataInstances.Select(m => m.SubscriberConfig).ToList();
            subscriberConfigurations.Should().HaveCountGreaterOrEqualTo(1).And.AllBeEquivalentTo(subscriberConfiguration);
        }

        /// <summary>
        /// Tests the service to determine that it can change role gracefully - while keeping messages and state inflight while migrating to the active secondaries.
        /// </summary>
        /// <returns></returns>
        [Theory(Skip = "no longer applicable as state is not being transferred")]
        [IsUnit]
        [InlineData("test.type", 1)]
        [InlineData("test.type", 10)]
        public async Task PromoteActivateSecondaryToPrimary(string eventName, int messageCount)
        {
            for (var x = 1; x <= messageCount; x++)
                _mockActorProxyFactory.RegisterActor(
                    CreateMockEventHandlerActor(new ActorId(string.Format(eventName + "-{0}", x))));

            var mockServiceBusManager = CreateMockServiceBusManager(eventName, messageCount);

            EventReaderService.EventReaderService Factory(StatefulServiceContext context, IReliableStateManagerReplica2 stateManager) =>
                new EventReaderService.EventReaderService(
                    context,
                    stateManager,
                    _mockedBigBrother,
                    mockServiceBusManager.Object,
                    _mockActorProxyFactory,
                    _config);

            var replicaSet = new MockStatefulServiceReplicaSet<EventReaderService.EventReaderService>(Factory, (context, dictionary) => new MockReliableStateManager(dictionary));

            //add a new Primary replica 
            await replicaSet.AddReplicaAsync(ReplicaRole.Primary, 1, initializationData: Encoding.UTF8.GetBytes("test.type"));

            //add a new ActiveSecondary replica
            await replicaSet.AddReplicaAsync(ReplicaRole.ActiveSecondary, 2, initializationData: Encoding.UTF8.GetBytes("test.type"));

            //add a second ActiveSecondary replica
            await replicaSet.AddReplicaAsync(ReplicaRole.ActiveSecondary, 3, initializationData: Encoding.UTF8.GetBytes("test.type"));

            void CheckInFlightMessagesOnPrimary()
            {
                var innerTask = Task.Run(async () =>
                {
                    while (replicaSet.Primary.ServiceInstance.InFlightMessageCount != messageCount)
                    {
                        await Task.Delay(100);
                    }
                });

                innerTask.Wait(TimeSpan.FromSeconds(2)).Should().BeTrue();
            }

            CheckInFlightMessagesOnPrimary();

            var oldPrimaryReplicaId = replicaSet.Primary.ReplicaId;

            await replicaSet.PromoteActiveSecondaryToPrimaryAsync(replicaSet.FirstActiveSecondary.ReplicaId);

            replicaSet.Primary.ReplicaId.Should().NotBe(oldPrimaryReplicaId);

            CheckInFlightMessagesOnPrimary();
        }

        private static IList<Message> CreateMessage(string eventName)
        {
            return new List<Message> { new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new MessageData("Hello World 1", eventName, "subA", "service")))) };
        }

        private static MockEventHandlerActor CreateMockEventHandlerActor(ActorId id)
        {
            ActorBase ActorFactory(ActorService service, ActorId actorId) => new MockEventHandlerActor(service, id);
            var svc = MockActorServiceFactory.CreateActorServiceForActor<MockEventHandlerActor>(ActorFactory);
            var actor = svc.Activate(id);
            return actor;
        }

        /// <summary>
        /// A mock of the handler for the ActorFactory which can be called
        /// </summary>
        private class MockEventHandlerActor : Actor, IEventHandlerActor
        {
            private readonly Queue<MessageData> _messageDataQueue = new Queue<MessageData>();

            public MockEventHandlerActor(ActorService actorService, ActorId actorId) : base(actorService, actorId)
            {
            }

            public IEnumerable<MessageData> MessageDataInstances => _messageDataQueue;

            public Task Handle(MessageData messageData)
            {
                _messageDataQueue.Enqueue(messageData);
                return Task.CompletedTask;
            }
        }
    }
}
