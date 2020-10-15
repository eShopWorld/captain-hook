using System.Runtime.Serialization;
using CaptainHook.Common.Configuration;

namespace CaptainHook.Application.Infrastructure.DirectorService.Remoting
{
    [KnownType(typeof(UpdateReader))]
    [KnownType(typeof(DeleteReader))]
    public abstract class ReaderChangeBase
    {
        public SubscriberConfiguration Subscriber { get; set; }

        protected ReaderChangeBase()
        {
        }

        protected ReaderChangeBase(SubscriberConfiguration subscriber)
        {
            Subscriber = subscriber;
        }
    }
}