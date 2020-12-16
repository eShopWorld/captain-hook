using System.Diagnostics.CodeAnalysis;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace CaptainHook.Common.Telemetry.Actor
{
    [ExcludeFromCodeCoverage]
    public class ActorDeactivated : ActorTelemetryEvent
    {
        public ActorDeactivated(ActorBase actor) : base(actor)
        {}
    }
}
