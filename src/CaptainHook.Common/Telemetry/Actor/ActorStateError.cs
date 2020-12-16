using System.Diagnostics.CodeAnalysis;
using Microsoft.ServiceFabric.Actors.Runtime;
using Newtonsoft.Json;

namespace CaptainHook.Common.Telemetry.Actor
{
    [ExcludeFromCodeCoverage]
    public class ActorStateError : ActorTelemetryEvent
    {
        public ActorStateError(object state, ActorBase actor) : base(actor)
        {
            State = JsonConvert.SerializeObject(state);
        }

        public string State;
    }
}