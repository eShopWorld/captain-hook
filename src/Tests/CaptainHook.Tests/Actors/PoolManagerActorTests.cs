using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eshopworld.Core;
using Eshopworld.Tests.Core;
using Microsoft.ServiceFabric.Actors;
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
            var actorGuid = Guid.NewGuid();
            var id = new ActorId(actorGuid);

            var actor = CreateActor(id, new Mock<IBigBrother>().Object);
            var stateManager = (MockActorStateManager)actor.StateManager;

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
            var actorGuid = Guid.NewGuid();
            var id = new ActorId(actorGuid);

            var actor = CreateActor(id, new Mock<IBigBrother>().Object);
            var stateManager = (MockActorStateManager)actor.StateManager;

            //create state
            var handle1 = await actor.DoWork(string.Empty, "test.type");
            var handle2 = await actor.DoWork(string.Empty, "test.type");

            //get state
            var actual = await stateManager.GetStateAsync<HashSet<int>>("_busy");
            Assert.Equal(18, actual.Count);
        }

        internal static PoolManagerActor.PoolManagerActor CreateActor(ActorId id, IBigBrother bb)
        {
            ActorBase ActorFactory(ActorService service, ActorId actorId) => new PoolManagerActor.PoolManagerActor(service, id, bb);
            var svc = MockActorServiceFactory.CreateActorServiceForActor<PoolManagerActor.PoolManagerActor>((Func<ActorService, ActorId, ActorBase>) ActorFactory);
            var actor = svc.Activate(id);
            return actor;
        }
    }
}
