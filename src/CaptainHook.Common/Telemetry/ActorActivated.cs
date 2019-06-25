using Microsoft.ServiceFabric.Actors.Runtime;

namespace CaptainHook.Common.Telemetry
{
    public class ActorActivated : ActorTelemetryEvent
    {
        public ActorActivated(ActorBase actor) : base(actor)
        {}
    }

    public class ActorError : ActorTelemetryEvent
    {
        public ActorError(string message, ActorBase actor) : base(actor)
        {
            Message = message;
        }

        public string Message { get; set; }
    }
}
