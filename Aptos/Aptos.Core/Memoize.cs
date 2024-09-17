using System.Collections.Concurrent;

namespace Aptos.Core
{
    static class Memoize
    {
        // The global cache Dictionary shared across all functions.
        // Must ensure that the cache keys are unique across all functions.
        private static readonly ConcurrentDictionary<string, (object value, long timestamp)> cache =
            new();

        /// <summary>
        /// A memoize higher-order function to cache async function response.
        /// </summary>
        /// <typeparam name="T">The type of the function result.</typeparam>
        /// <param name="func">An async function to cache the result of.</param>
        /// <param name="key">The provided cache key.</param>
        /// <param name="ttlMs">Time-to-live in milliseconds for cached data.</param>
        /// <returns>The cached or latest result.</returns>
        public static Func<Task<T>> MemoAsync<T>(Func<Task<T>> func, string key, long? ttlMs = null)
        {
            return async () =>
            {
                // Check if the cached result exists and is within TTL
                if (cache.TryGetValue(key, out var cacheEntry))
                {
                    var (value, timestamp) = cacheEntry;
                    if (
                        ttlMs == null
                        || DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - timestamp <= ttlMs.Value
                    )
                    {
                        return (T)value;
                    }
                }

                // If not cached or TTL expired, compute the result
                var result = await func();

                // Cache the result with a timestamp if it's not null
                if (result != null)
                    cache[key] = (result, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

                return result;
            };
        }

        /// <summary>
        /// A memoize higher-order function to cache function response.
        /// </summary>
        /// <typeparam name="T">The type of the function result.</typeparam>
        /// <param name="func">A function to cache the result of.</param>
        /// <param name="key">The provided cache key.</param>
        /// <param name="ttlMs">Time-to-live in milliseconds for cached data.</param>
        /// <returns>The cached or latest result.</returns>
        public static Func<T> Memo<T>(Func<T> func, string key, long? ttlMs = null)
        {
            return () =>
            {
                // Check if the cached result exists and is within TTL
                if (cache.TryGetValue(key, out var cacheEntry))
                {
                    var (value, timestamp) = cacheEntry;
                    if (
                        ttlMs == null
                        || DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - timestamp <= ttlMs.Value
                    )
                    {
                        return (T)value;
                    }
                }

                // If not cached or TTL expired, compute the result
                var result = func();

                // Cache the result with a timestamp
                if (result != null)
                    cache[key] = (result, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

                return result;
            };
        }
    }
}
