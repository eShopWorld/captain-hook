using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CaptainHook.EventReaderService
{
    public static class CollectionExtensions
    {
        public static ConcurrentQueue<T> ToConcurrentQueue<T>(this IEnumerable<T> set)
        {
            return new ConcurrentQueue<T>(set);
        }
    }
}