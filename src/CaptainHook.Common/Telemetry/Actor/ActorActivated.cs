using System.Diagnostics.CodeAnalysis;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace CaptainHook.Common.Telemetry.Actor
{
    [ExcludeFromCodeCoverage]
    public class ActorActivated : ActorTelemetryEvent
    {
        public ActorActivated(ActorBase actor) : base(actor)
        {}
    }
}
