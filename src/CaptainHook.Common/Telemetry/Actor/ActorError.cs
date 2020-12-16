using System.Diagnostics.CodeAnalysis;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace CaptainHook.Common.Telemetry.Actor
{
    [ExcludeFromCodeCoverage]
    public class ActorError : ActorTelemetryEvent
    {
        public ActorError(string message, ActorBase actor) : base(actor)
        {
            Message = message;
        }

        public string Message { get; set; }
    }
}