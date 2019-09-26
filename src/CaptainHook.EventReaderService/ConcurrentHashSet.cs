using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CaptainHook.EventReaderService
{
    /// <summary>
    /// A Semephore based lock around a HashSet to make the HashSet ThreadSafe
    /// Ensures only one thread access the HashSet at a time.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConcurrentHashSet<T>
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly HashSet<T> _set;


        public ConcurrentHashSet()
        {
            _set = new HashSet<T>();
        }

        public ConcurrentHashSet(IEnumerable<T> set)
        {
            _set = new HashSet<T>(set);
        }

        /// <summary>
        /// Creates a Hashset from the internal stored HashSet
        /// </summary>
        public HashSet<T> ToHashSet => _set.ToHashSet();

        public async Task<T> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
        {
            return await EnterSemaphoreAsync<T>(() => _set.FirstOrDefault(), cancellationToken);
        }

        public async Task AddAsync(T value, CancellationToken cancellationToken = default)
        {
            await EnterSemaphoreAsync(() => { _set.Add(value); }, cancellationToken);
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await EnterSemaphoreAsync(() => _set.Count(), cancellationToken);
        }

        public async Task RemoveAsync(T value, CancellationToken cancellationToken = default)
        {
            await EnterSemaphoreAsync(() => { _set.Remove(value); }, cancellationToken);
        }
        
        private async Task EnterSemaphoreAsync(Action action, CancellationToken cancellationToken)
        {
            try
            {
                await _semaphore.WaitAsync(cancellationToken);

                action();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task<P> EnterSemaphoreAsync<P>(Func<P> func, CancellationToken cancellationToken)
        {
            try
            {
                await _semaphore.WaitAsync(cancellationToken);

                return func();
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}