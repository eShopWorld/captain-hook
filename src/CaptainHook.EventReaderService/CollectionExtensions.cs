using System.Collections.Generic;

namespace CaptainHook.EventReaderService
{
    public static class CollectionExtensions
    {
        public static ConcurrentHashSet<T> ToConcurrentHashSet<T>(this IEnumerable<T> set)
        {
            return new ConcurrentHashSet<T>(set);
        }
    }
}