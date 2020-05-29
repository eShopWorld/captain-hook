using System;
using System.Linq;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.EventHandlerActor.Handlers;
using Eshopworld.Core;
using Eshopworld.Tests.Core;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Moq;
using ServiceFabric.Mocks;
using Xunit;

namespace CaptainHook.Tests.Services.Actors
{
    public class EventHandlerActorTests
    {
        [Fact]
        [IsLayer0]
        public async Task CheckHasTimerAfterHandleCall()
        {
            var bigBrotherMock = new Mock<IBigBrother>().Object;

            var eventHandlerActor = CreateEventHandlerActor(new ActorId(1), bigBrotherMock);

            await eventHandlerActor.Handle(new MessageData(string.Empty, "test.type", "subA", "service"));

            var timers = eventHandlerActor.GetActorTimers();
            Assert.True(timers.Any());
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
