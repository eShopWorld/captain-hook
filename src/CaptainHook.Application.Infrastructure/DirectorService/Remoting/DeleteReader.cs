using CaptainHook.Common.Configuration;

namespace CaptainHook.Application.Infrastructure.DirectorService.Remoting
{
    public class DeleteReader : ReaderChangeBase
    {
        public DeleteReader() : base()
        {
        }

        public DeleteReader(SubscriberConfiguration subscriber) : base(subscriber)
        {
        }
    }
}