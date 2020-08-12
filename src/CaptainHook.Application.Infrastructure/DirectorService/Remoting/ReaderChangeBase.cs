using System.Runtime.Serialization;
using CaptainHook.Common.Configuration;

namespace CaptainHook.Application.Infrastructure.DirectorService.Remoting
{
    [KnownType(typeof(CreateReader))]
    [KnownType(typeof(UpdateReader))]
    public abstract class ReaderChangeBase
    {
        public SubscriberConfiguration Subscriber { get; set; }
    }
}