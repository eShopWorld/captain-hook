using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CaptainHook.Common.Configuration;
using CaptainHook.EventHandlerActor.Handlers;
using CaptainHook.Interfaces;
using Eshopworld.Core;
using Eshopworld.Telemetry;
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
            var bigBrotherMock = new Mock<IBigBrother>().Object;

            var actorProxyFactory = new MockActorProxyFactory();
            var eventHandlerActor = CreateEventHandlerActor(new ActorId(1), bigBrotherMock);
            actorProxyFactory.RegisterActor(eventHandlerActor);

            var eventReaderActor = CreateEventReaderActor(new ActorId("test.type"), bigBrotherMock);
            actorProxyFactory.RegisterActor(eventReaderActor);

            var actor = CreatePoolManagerActor(new ActorId(0), bigBrotherMock, actorProxyFactory);
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
            var bigBrotherMock = new Mock<IBigBrother>().Object;

            var actorProxyFactory = new MockActorProxyFactory();
            var eventHandlerActor = CreateEventHandlerActor(new ActorId(0), bigBrotherMock);
            actorProxyFactory.RegisterActor(eventHandlerActor);

            var actor = CreatePoolManagerActor(new ActorId(0), bigBrotherMock, actorProxyFactory);
            var stateManager = (MockActorStateManager)actor.StateManager;
            await actor.InvokeOnActivateAsync();

            //create state
            var handle = await actor.DoWork(string.Empty, "test.type");

            await actor.CompleteWork(handle);
            //get state
            var actual = await stateManager.GetStateAsync<HashSet<int>>("_busy");
            Assert.Equal(18, actual.Count);
        }

        private static PoolManagerActor.PoolManagerActor CreatePoolManagerActor(ActorId id, IBigBrother bb, IActorProxyFactory mockActorProxyFactory)
        {
            ActorBase ActorFactory(ActorService service, ActorId actorId) => new PoolManagerActor.PoolManagerActor(service, id, bb, mockActorProxyFactory);
            var svc = MockActorServiceFactory.CreateActorServiceForActor<PoolManagerActor.PoolManagerActor>(ActorFactory);
            var actor = svc.Activate(id);
            return actor;
        }

        private static IEventHandlerActor CreateEventHandlerActor(ActorId id, IBigBrother bb)
        {
            ActorBase ActorFactory(ActorService service, ActorId actorId) => new EventHandlerActor.EventHandlerActor(service, id, new Mock<IEventHandlerFactory>().Object, bb);
            var svc = MockActorServiceFactory.CreateActorServiceForActor<EventHandlerActor.EventHandlerActor>(ActorFactory);
            var actor = svc.Activate(id);
            return actor;
        }

        private static IEventReaderActor CreateEventReaderActor(ActorId id, IBigBrother bb)
        {
            ActorBase ActorFactory(ActorService service, ActorId actorId) => new EventReaderActor.EventReaderActor(service, id, bb, new ConfigurationSettings());
            var svc = MockActorServiceFactory.CreateActorServiceForActor<EventReaderActor.EventReaderActor>(ActorFactory);
            var actor = svc.Activate(id);
            return actor;
        }
    }
}
