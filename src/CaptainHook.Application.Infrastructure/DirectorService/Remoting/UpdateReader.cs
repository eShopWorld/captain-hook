using CaptainHook.Common.Configuration;

namespace CaptainHook.Application.Infrastructure.DirectorService.Remoting
{
    public class UpdateReader : ReaderChangeBase
    {
        public UpdateReader() : base()
        {
        }

        public UpdateReader(SubscriberConfiguration subscriber) : base(subscriber)
        {
        }
    }
}