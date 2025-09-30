using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Cache.Core;

public class InMemoryCache(ILogger logger, InMemoryCacheOptions? options = null)
{
    private readonly ILogger _logger = logger;
    private readonly InMemoryCacheOptions? _options = options;
    private readonly ConcurrentDictionary<object, object?> _data = [];
    private readonly SemaphoreSlim _semaphore = new(1);
    private readonly LinkedList<object> _accessedList = [];

    public bool TryGet(object key, out object? value)
    {
        if (_data.TryGetValue(key, out value))
        {
            // Hit, update accessed list to move it to front of queue
            try
            {
                UpdateKeyUsed(key);
                _logger.LogInformation($"Cache hit on key: [{key}]");
                return true;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // Miss
        _logger.LogInformation($"Cache miss on key: [{key}]");
        return false;
    }

    public void Set(object key, object? value)
    {
        // If someone is hitting an API request multiple times and each one comes through at the same time while waiting for semaphore,
        // we want to ensure we update the dictionary without throwing an exception and cache only one value.
        _data[key] = value;
        UpdateKeyUsed(key);
    }

    private void UpdateKeyUsed(object key)
    {
        if ((_options?.MaxNumItems ?? 0)  <= 0)
        {
            // Early return if `disabled`
            return;
        }

        // Not thread safe so using semaphore
        _semaphore.Wait();

        // Key could always previously exist. O(n) but in best case (frequently accessed data) is closer to constant time.
        _accessedList.Remove(key);

        // Remove keys if limit hit
        if (_accessedList.Count >= (_options?.MaxNumItems ?? long.MaxValue))
        {
            var lastAccessedKey = _accessedList.Last();
            _logger.LogInformation($"Evicted key: [{lastAccessedKey}]");
            _accessedList.RemoveLast();
        }

        _accessedList.AddFirst(key);
        _semaphore.Release();
    }
}
