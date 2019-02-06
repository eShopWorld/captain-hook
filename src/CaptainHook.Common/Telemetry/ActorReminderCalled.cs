using Microsoft.ServiceFabric.Actors.Runtime;

namespace CaptainHook.Common.Telemetry
{
    public class ActorReminderCalled : ActorActivated
    {
        public ActorReminderCalled(ActorBase actor, string reminderName) : base(actor)
        {
            this.ActorId = actor.Id.ToString();
            this.ActorName = actor.ActorService.ActorTypeInformation.ServiceName;
            ReminderName = reminderName;
        }

        public string ReminderName { get; set; }
    }
}