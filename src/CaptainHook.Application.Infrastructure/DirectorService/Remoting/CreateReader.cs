using CaptainHook.Common.Configuration;

namespace CaptainHook.Application.Infrastructure.DirectorService.Remoting
{
    public class CreateReader : ReaderChangeBase
    {
        public CreateReader() : base()
        {
        }

        public CreateReader(SubscriberConfiguration subscriber) : base(subscriber)
        {
        }
    }
}