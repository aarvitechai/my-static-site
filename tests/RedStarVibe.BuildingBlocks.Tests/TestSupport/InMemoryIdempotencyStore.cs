using RedStarVibe.BuildingBlocks.Web.Idempotency;

namespace RedStarVibe.BuildingBlocks.Tests.TestSupport;

/// <summary>Test double for the Redis store: same contract (atomic claim), no TTLs.</summary>
internal sealed class InMemoryIdempotencyStore : IIdempotencyStore
{
    private readonly Lock _gate = new();
    private readonly Dictionary<string, (string Fingerprint, IdempotentResponse? Response)> _entries = [];

    public int Count
    {
        get
        {
            lock (_gate)
            {
                return _entries.Count;
            }
        }
    }

    public string? FingerprintOf(string scopedKey)
    {
        lock (_gate)
        {
            return _entries.TryGetValue(scopedKey, out var entry) ? entry.Fingerprint : null;
        }
    }

    /// <summary>Simulate another request with the same key currently being processed.</summary>
    public void SeedInProgress(string scopedKey, string fingerprint)
    {
        lock (_gate)
        {
            _entries[scopedKey] = (fingerprint, null);
        }
    }

    public Task<IdempotencyBeginResult> TryBeginAsync(string key, string fingerprint, TimeSpan lockTtl, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            if (!_entries.TryGetValue(key, out var entry))
            {
                _entries[key] = (fingerprint, null);
                return Task.FromResult(new IdempotencyBeginResult(IdempotencyBeginStatus.Started));
            }

            if (entry.Fingerprint != fingerprint)
            {
                return Task.FromResult(new IdempotencyBeginResult(IdempotencyBeginStatus.Mismatch));
            }

            return Task.FromResult(entry.Response is null
                ? new IdempotencyBeginResult(IdempotencyBeginStatus.InProgress)
                : new IdempotencyBeginResult(IdempotencyBeginStatus.Completed, entry.Response));
        }
    }

    public Task CompleteAsync(string key, string fingerprint, IdempotentResponse response, TimeSpan ttl, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            _entries[key] = (fingerprint, response);
        }

        return Task.CompletedTask;
    }

    public Task ReleaseAsync(string key, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            _entries.Remove(key);
        }

        return Task.CompletedTask;
    }
}
