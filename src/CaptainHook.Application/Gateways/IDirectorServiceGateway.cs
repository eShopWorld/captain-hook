using System.Threading.Tasks;
using CaptainHook.Domain.Entities;

namespace CaptainHook.Application.Gateways
{
    public interface IDirectorServiceGateway
    {
        Task<CreateReaderResult> CreateReader(SubscriberEntity readerChangeInfo);
    }

    public enum CreateReaderResult
    {
        Unknown,
        Started,
        Failure,
        Busy
    }
}
