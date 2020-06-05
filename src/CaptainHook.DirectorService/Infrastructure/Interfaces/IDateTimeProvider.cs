using System;

namespace CaptainHook.DirectorService.Infrastructure.Interfaces
{
    public interface IDateTimeProvider
    {
        DateTimeOffset UtcNow { get; }
    }
}
