﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using CaptainHook.EventHandlerActor.Handlers;
using CaptainHook.EventReaderService;
using Eshopworld.Core;
using Eshopworld.Tests.Core;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Moq;
using ServiceFabric.Mocks;
using Xunit;

namespace CaptainHook.Tests.Services.Actors
{
    public class MessageProviderFactoryMock : IMessageProviderFactory
    {
        public IMessageReceiver Builder(string serviceBusConnectionString, string topicName, string subscription)
        {
            return new MessageReceiverMock();
        }

        public int ReceiverBatchSize { get; set; }
        public int BackoffMin { get; set; }
        public int BackoffMax { get; set; }
    }

    public class MessageReceiverMock : IMessageReceiver
    {
        public Task CloseAsync()
        {
            throw new NotImplementedException();
        }

        public void RegisterPlugin(ServiceBusPlugin serviceBusPlugin)
        {
            throw new NotImplementedException();
        }

        public void UnregisterPlugin(string serviceBusPluginName)
        {
            throw new NotImplementedException();
        }

        public string ClientId { get; }

        public bool IsClosedOrClosing { get; }
        public string Path { get; }
        public TimeSpan OperationTimeout { get; set; }
        public ServiceBusConnection ServiceBusConnection { get; }
        public bool OwnsConnection { get; }
        public IList<ServiceBusPlugin> RegisteredPlugins { get; }
        public void RegisterMessageHandler(Func<Message, CancellationToken, Task> handler, Func<ExceptionReceivedEventArgs, Task> exceptionReceivedHandler)
        {
            throw new NotImplementedException();
        }

        public void RegisterMessageHandler(Func<Message, CancellationToken, Task> handler, MessageHandlerOptions messageHandlerOptions)
        {
            throw new NotImplementedException();
        }

        public Task CompleteAsync(string lockToken)
        {
            throw new NotImplementedException();
        }

        public Task AbandonAsync(string lockToken, IDictionary<string, object> propertiesToModify = null)
        {
            throw new NotImplementedException();
        }

        public Task DeadLetterAsync(string lockToken, IDictionary<string, object> propertiesToModify = null)
        {
            throw new NotImplementedException();
        }

        public Task DeadLetterAsync(string lockToken, string deadLetterReason, string deadLetterErrorDescription = null)
        {
            throw new NotImplementedException();
        }

        public int PrefetchCount { get; set; }
        public ReceiveMode ReceiveMode { get; }
        public Task<Message> ReceiveAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Message> ReceiveAsync(TimeSpan operationTimeout)
        {
            throw new NotImplementedException();
        }

        public Task<IList<Message>> ReceiveAsync(int maxMessageCount)
        {
            throw new NotImplementedException();
        }

        public Task<IList<Message>> ReceiveAsync(int maxMessageCount, TimeSpan operationTimeout)
        {
            throw new NotImplementedException();
        }

        public Task<Message> ReceiveDeferredMessageAsync(long sequenceNumber)
        {
            throw new NotImplementedException();
        }

        public Task<IList<Message>> ReceiveDeferredMessageAsync(IEnumerable<long> sequenceNumbers)
        {
            throw new NotImplementedException();
        }

        public Task CompleteAsync(IEnumerable<string> lockTokens)
        {
            throw new NotImplementedException();
        }

        public Task DeferAsync(string lockToken, IDictionary<string, object> propertiesToModify = null)
        {
            throw new NotImplementedException();
        }

        public Task RenewLockAsync(Message message)
        {
            throw new NotImplementedException();
        }

        public Task<DateTime> RenewLockAsync(string lockToken)
        {
            throw new NotImplementedException();
        }

        public Task<Message> PeekAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IList<Message>> PeekAsync(int maxMessageCount)
        {
            throw new NotImplementedException();
        }

        public Task<Message> PeekBySequenceNumberAsync(long fromSequenceNumber)
        {
            throw new NotImplementedException();
        }

        public Task<IList<Message>> PeekBySequenceNumberAsync(long fromSequenceNumber, int messageCount)
        {
            throw new NotImplementedException();
        }

        public long LastPeekedSequenceNumber { get; }
    }


    public class EventReaderTests
    {
        public async Task CanGetMessages()
        {
            var context = MockStatefulServiceContextFactory.Default;
            var stateManager = new MockReliableStateManager();
            var mockedBigBrother = new Mock<IBigBrother>();
            var config = new ConfigurationSettings();
            var messageProviderFactory = new MessageProviderFactoryMock();
            var service = new EventReaderService.EventReaderService(context, mockedBigBrother.Object, messageProviderFactory, config);

            await service.InvokeRunAsync(CancellationToken.None);

            //mock the service bus provider
        }

        [Fact]
        [IsLayer0]
        public async Task CanCancel()
        {

        }

        public async Task ReaderDoesntGetMessagesWhenHandlersAreFull()
        {

        }

        public async Task CanDeleteMessageFromSubscription()
        {

        }

        public async Task CheckHandlesInInMemoryState()
        {

        }
    }

    public class DirectorServiceTests
    {
        public async Task CanCreateReaders()
        {

        }

        public async Task CanCreateHandler()
        {

        }
    }

    public class EventHandlerActorTests
    {
        [Fact]
        [IsLayer0]
        public async Task CheckHasTimerAfterHandleCall()
        {
            var bigBrotherMock = new Mock<IBigBrother>().Object;

            var eventHandlerActor = CreateEventHandlerActor(new ActorId(1), bigBrotherMock);
            
            await eventHandlerActor.Handle(new MessageData(string.Empty, "test.type"));

            var timers = eventHandlerActor.GetActorTimers();
            Assert.True(timers.Any());
        }

        //todo
        [Theory]
        [IsLayer0]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CallReaderToCompleteMessage(bool expectedMessageDelivered)
        {

        }

        private static EventHandlerActor.EventHandlerActor CreateEventHandlerActor(ActorId id, IBigBrother bigBrother)
        {
            ActorBase ActorFactory(ActorService service, ActorId actorId) => new EventHandlerActor.EventHandlerActor(service, id, new Mock<IEventHandlerFactory>().Object, bigBrother);
            var svc = MockActorServiceFactory.CreateActorServiceForActor<EventHandlerActor.EventHandlerActor>(ActorFactory);
            var actor = svc.Activate(id);
            return actor;
        }
    }
}