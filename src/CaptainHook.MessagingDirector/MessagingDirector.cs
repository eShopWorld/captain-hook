namespace CaptainHook.MessagingDirector
{
    using System.Threading.Tasks;
    using Interfaces;
    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Actors.Client;
    using Microsoft.ServiceFabric.Actors.Runtime;

    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    public class MessagingDirector : Actor, IMessagingDirector
    {
        private const string MessageTypesKey = "MessageTypes";

        /// <summary>
        /// Initializes a new instance of <see cref="MessagingDirector"/>.
        /// </summary>
        /// <param name="actorService">The <see cref="ActorService"/> that will host this actor instance.</param>
        /// <param name="actorId">The <see cref="ActorId"/> for this actor instance.</param>
        public MessagingDirector(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        /// <inheritdoc />
        protected override async Task OnActivateAsync()
        {
            await StateManager.TryAddStateAsync(MessageTypesKey,
                new[]
                {
                    "checkout.domain.infrastructure.domainevents.retailerorderconfirmationdomainevent",
                    "checkout.domain.infrastructure.domainevents.platformordercreatedomainevent",
                    "nike.snkrs.core.events.productrefreshevent",
                    "nike.snkrs.core.events.productupdatedevent"
                });
        }

        public async Task Run()
        {
            var types = await StateManager.TryGetStateAsync<string[]>(MessageTypesKey);

            foreach (var type in types.Value)
            {
                await ActorProxy.Create<IEventReaderActor>(new ActorId(type)).Run();
            }
        }
    }
}
