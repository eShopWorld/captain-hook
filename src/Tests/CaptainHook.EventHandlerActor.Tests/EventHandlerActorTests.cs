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

namespace CaptainHook.EventHandlerActor.Tests
{
    public class EventHandlerActorTests
    {
        [Fact]
        [IsUnit]
        public async Task CheckHasTimerAfterHandleCall()
        {
            var bigBrotherMock = new Mock<IBigBrother>().Object;

            var eventHandlerActor = CreateEventHandlerActor(new ActorId(1), bigBrotherMock);

            await eventHandlerActor.Handle(new MessageData(string.Empty, "test.type", "subA", "service"));

            var timers = eventHandlerActor.GetActorTimers();
            Assert.True(timers.Any());
        }

        private static EventHandlerActor CreateEventHandlerActor(ActorId id, IBigBrother bigBrother)
        {
            ActorBase ActorFactory(ActorService service, ActorId actorId) => new EventHandlerActor(service, id, new Mock<IEventHandlerFactory>().Object, bigBrother);
            var svc = MockActorServiceFactory.CreateActorServiceForActor<EventHandlerActor>(ActorFactory);
            var actor = svc.Activate(id);
            return actor;
        }
    }
}
