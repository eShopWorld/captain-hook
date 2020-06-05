using CaptainHook.DirectorService.Infrastructure.Interfaces;
using System;

namespace CaptainHook.DirectorService.Infrastructure
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
