using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.Interfaces;
using Eshopworld.Core;
using Eshopworld.Tests.Core;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Runtime;
using Moq;
using ServiceFabric.Mocks;
using Xunit;

namespace CaptainHook.Tests.Actors
{
    public class PoolManagerActorTests
    {
        [Fact]
        [IsLayer0]
        public async Task CheckFreeHandlers()
        {
            //setup the actors
            var bigBrotherMock = new Mock<IBigBrother>().Object;

            var mockActorProxyFactory = new MockActorProxyFactory();
            var eventHandlerActor = CreateMockEventHandlerActor(new ActorId(1), bigBrotherMock);
            mockActorProxyFactory.RegisterActor(eventHandlerActor);

            var eventReaderActor = CreateMockEventReaderActor(new ActorId("test.type"), bigBrotherMock);
            mockActorProxyFactory.RegisterActor(eventReaderActor);


            var actor = CreatePoolManagerActor(new ActorId(0), bigBrotherMock, mockActorProxyFactory);
            var stateManager = (MockActorStateManager)actor.StateManager;
            await actor.InvokeOnActivateAsync();

            //create state
            var handle = await actor.DoWork(string.Empty, "test.type");

            await actor.CompleteWork(handle);

            //get state
            var actual = await stateManager.GetStateAsync<HashSet<int>>("_free");
            Assert.Equal(20, actual.Count);
        }

        [Fact]
        [IsLayer0]
        public async Task CheckBusyHandlers()
        {
            //setup the actors
            var bigBrotherMock = new Mock<IBigBrother>().Object;

            var actorProxyFactory = new MockActorProxyFactory();
            var eventHandlerActor1 = CreateMockEventHandlerActor(new ActorId(1), bigBrotherMock);
            actorProxyFactory.RegisterActor(eventHandlerActor1);

            var eventHandlerActor2 = CreateMockEventHandlerActor(new ActorId(2), bigBrotherMock);
            actorProxyFactory.RegisterActor(eventHandlerActor2);

            var actor = CreatePoolManagerActor(new ActorId("test.type"), bigBrotherMock, actorProxyFactory);
            var stateManager = (MockActorStateManager)actor.StateManager;
            await actor.InvokeOnActivateAsync();

            //create state
            await actor.DoWork(string.Empty, "test.type1");
            await actor.DoWork(string.Empty, "test.type2");

            //get state
            var actual = await stateManager.GetStateAsync<Dictionary<Guid, MessageHook>>("_busy");
            Assert.Equal(2, actual.Count);
        }

        [Fact]
        [IsLayer0]
        public async Task CheckBadHandler()
        {
            //setup the actors
            var bigBrotherMock = new Mock<IBigBrother>().Object;

            var actorProxyFactory = new MockActorProxyFactory();
            var eventHandlerActor1 = CreateMockEventHandlerWithExceptionActor(new ActorId(1), bigBrotherMock);
            actorProxyFactory.RegisterActor(eventHandlerActor1);

            var eventHandlerActor2 = CreateMockEventHandlerActor(new ActorId(2), bigBrotherMock);
            actorProxyFactory.RegisterActor(eventHandlerActor2);

            var actor = CreatePoolManagerActor(new ActorId("test.type"), bigBrotherMock, actorProxyFactory);
            var stateManager = (MockActorStateManager)actor.StateManager;
            await actor.InvokeOnActivateAsync();

            //create state
            await actor.DoWork(string.Empty, "test.type1");

            //get state
            var actualBusy = await stateManager.GetStateAsync<Dictionary<Guid, MessageHook>>("_busy");
            Assert.Empty(actualBusy);

            var actualFree = await stateManager.GetStateAsync<Dictionary<Guid, MessageHook>>("_free");
            Assert.Equal(20, actualFree.Count);
        }

        private static PoolManagerActor.PoolManagerActor CreatePoolManagerActor(ActorId id, IBigBrother bb, IActorProxyFactory mockActorProxyFactory)
        {
            ActorBase ActorFactory(ActorService service, ActorId actorId) => new PoolManagerActor.PoolManagerActor(service, id, bb, mockActorProxyFactory);
            var svc = MockActorServiceFactory.CreateActorServiceForActor<PoolManagerActor.PoolManagerActor>(ActorFactory);
            var actor = svc.Activate(id);
            return actor;
        }

        private static IEventHandlerActor CreateMockEventHandlerActor(ActorId id, IBigBrother bb)
        {
            ActorBase ActorFactory(ActorService service, ActorId actorId) => new MockEventHandlerActor(service, id);
            var svc = MockActorServiceFactory.CreateActorServiceForActor<MockEventHandlerActor>(ActorFactory);
            var actor = svc.Activate(id);
            return actor;
        }

        private static IEventReaderActor CreateMockEventReaderActor(ActorId id, IBigBrother bb)
        {
            ActorBase ActorFactory(ActorService service, ActorId actorId) => new MockEventReaderActor(service, id);
            var svc = MockActorServiceFactory.CreateActorServiceForActor<MockEventReaderActor>(ActorFactory);
            var actor = svc.Activate(id);
            return actor;
        }

        private static IEventHandlerActor CreateMockEventHandlerWithExceptionActor(ActorId id, IBigBrother bb)
        {
            ActorBase ActorFactory(ActorService service, ActorId actorId) => new MockEventHandlerActorWithException(service, id);
            var svc = MockActorServiceFactory.CreateActorServiceForActor<MockEventHandlerActorWithException>(ActorFactory);
            var actor = svc.Activate(id);
            return actor;
        }

        private class MockEventHandlerActor : Actor, IEventHandlerActor
        {
            public MockEventHandlerActor(ActorService actorService, ActorId actorId) : base(actorService, actorId)
            {
            }

            public Task Handle(Guid handle, string payload, string type)
            {
                return Task.FromResult(true);
            }

            public Task CompleteHandle(Guid handle)
            {
                return Task.FromResult(true);
            }
        }

        private class MockEventHandlerActorWithException : Actor, IEventHandlerActor
        {
            public MockEventHandlerActorWithException(ActorService actorService, ActorId actorId) : base(actorService, actorId)
            {
            }

            public Task Handle(Guid handle, string payload, string type)
            {
                throw new Exception();
            }

            public Task CompleteHandle(Guid handle)
            {
                return Task.FromResult(true);
            }
        }

        private class MockEventReaderActor : Actor, IEventReaderActor
        {
            public MockEventReaderActor(ActorService actorService, ActorId actorId) : base(actorService, actorId)
            {
            }

            public Task Run()
            {
                return Task.FromResult(true);
            }

            public Task CompleteMessage(Guid handle)
            {
                return Task.FromResult(true);
            }
        }
    }
}
