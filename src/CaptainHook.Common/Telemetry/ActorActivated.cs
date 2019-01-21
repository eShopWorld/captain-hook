using Eshopworld.Core;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace CaptainHook.Common.Telemetry
{
    public class ActorActivated : TelemetryEvent
    {
        public string ActorName { get; set; }

        public string ActorId { get; set; }

        public ActorActivated(ActorBase actor)
        {
            ActorId = actor.Id.ToString();
            ActorName = actor.ActorService.ActorTypeInformation.ServiceName;
        }
    }
}
