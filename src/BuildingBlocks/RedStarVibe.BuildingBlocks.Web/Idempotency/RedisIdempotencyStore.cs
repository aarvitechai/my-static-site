using System.Text.Json;
using StackExchange.Redis;

namespace RedStarVibe.BuildingBlocks.Web.Idempotency;

/// <summary>
/// Redis-backed store. "SET key value NX" is atomic, so two simultaneous requests with the same key can never
/// both start processing, even across several API instances.
/// </summary>
internal sealed class RedisIdempotencyStore(IConnectionMultiplexer redis) : IIdempotencyStore
{
    private const string Prefix = "rsv:idempotency:";

    private sealed record Entry(string Fingerprint, bool Completed, IdempotentResponse? Response);

    public async Task<IdempotencyBeginResult> TryBeginAsync(string key, string fingerprint, TimeSpan lockTtl, CancellationToken cancellationToken)
    {
        var db = redis.GetDatabase();
        var redisKey = new RedisKey(Prefix + key);

        // Two attempts: the existing entry can expire between our SET and GET.
        for (var attempt = 0; attempt < 2; attempt++)
        {
            var inProgress = JsonSerializer.Serialize(new Entry(fingerprint, Completed: false, Response: null));
            if (await db.StringSetAsync(redisKey, inProgress, lockTtl, When.NotExists))
            {
                return new IdempotencyBeginResult(IdempotencyBeginStatus.Started);
            }

            var existing = await db.StringGetAsync(redisKey);
            if (existing.IsNullOrEmpty)
            {
                continue;
            }

            var entry = JsonSerializer.Deserialize<Entry>(existing.ToString());
            if (entry is null || entry.Fingerprint != fingerprint)
            {
                return new IdempotencyBeginResult(IdempotencyBeginStatus.Mismatch);
            }

            return entry.Completed
                ? new IdempotencyBeginResult(IdempotencyBeginStatus.Completed, entry.Response)
                : new IdempotencyBeginResult(IdempotencyBeginStatus.InProgress);
        }

        return new IdempotencyBeginResult(IdempotencyBeginStatus.InProgress);
    }

    public Task CompleteAsync(string key, string fingerprint, IdempotentResponse response, TimeSpan ttl, CancellationToken cancellationToken) =>
        redis.GetDatabase().StringSetAsync(
            Prefix + key,
            JsonSerializer.Serialize(new Entry(fingerprint, Completed: true, response)),
            ttl);

    public Task ReleaseAsync(string key, CancellationToken cancellationToken) =>
        redis.GetDatabase().KeyDeleteAsync(Prefix + key);
}
