using Microsoft.ServiceFabric.Actors.Runtime;
using Newtonsoft.Json;

namespace CaptainHook.Common.Telemetry.Actor
{
    public class ActorHandleStarted : ActorTelemetryEvent
    {
        public ActorHandleStarted(MessageData messageData, ActorBase actor) : base(actor)
        {
            MessageData = JsonConvert.SerializeObject(messageData);
        }

        public string MessageData;
    }
}