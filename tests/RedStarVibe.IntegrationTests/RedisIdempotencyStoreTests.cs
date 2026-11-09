using System.Text;
using RedStarVibe.BuildingBlocks.Web.Idempotency;
using RedStarVibe.IntegrationTests.Infrastructure;
using StackExchange.Redis;

namespace RedStarVibe.IntegrationTests;

/// <summary>The production idempotency store against a real Redis (slice 4 only tested it through an in-memory double).</summary>
public sealed class RedisIdempotencyStoreTests(ContainersFixture containers) : IAsyncLifetime
{
    private static readonly TimeSpan Lock = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan Keep = TimeSpan.FromHours(1);
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private ConnectionMultiplexer _redis = null!;
    private RedisIdempotencyStore _store = null!;

    public async ValueTask InitializeAsync()
    {
        _redis = await ConnectionMultiplexer.ConnectAsync(containers.RedisConnectionString);
        _store = new RedisIdempotencyStore(_redis);
    }

    public async ValueTask DisposeAsync() => await _redis.DisposeAsync();

    private static string NewKey() => "test:" + Guid.NewGuid().ToString("N");

    private static IdempotentResponse Created(string body) =>
        new(201, "application/json", "/api/v1/things/1", Encoding.UTF8.GetBytes(body));

    [Fact]
    public async Task FirstClaimStartsAndSecondSeesInProgress()
    {
        var key = NewKey();

        var first = await _store.TryBeginAsync(key, "fp", Lock, Ct);
        var second = await _store.TryBeginAsync(key, "fp", Lock, Ct);

        Assert.Equal(IdempotencyBeginStatus.Started, first.Status);
        Assert.Equal(IdempotencyBeginStatus.InProgress, second.Status);
    }

    [Fact]
    public async Task CompletedResponseIsReplayedExactly()
    {
        var key = NewKey();
        await _store.TryBeginAsync(key, "fp", Lock, Ct);
        await _store.CompleteAsync(key, "fp", Created("""{"id":1}"""), Keep, Ct);

        var replay = await _store.TryBeginAsync(key, "fp", Lock, Ct);

        Assert.Equal(IdempotencyBeginStatus.Completed, replay.Status);
        Assert.NotNull(replay.Response);
        Assert.Equal(201, replay.Response.StatusCode);
        Assert.Equal("/api/v1/things/1", replay.Response.Location);
        Assert.Equal("""{"id":1}""", Encoding.UTF8.GetString(replay.Response.Body));
    }

    [Fact]
    public async Task DifferentFingerprintIsAMismatch()
    {
        var key = NewKey();
        await _store.TryBeginAsync(key, "fp-a", Lock, Ct);

        var other = await _store.TryBeginAsync(key, "fp-b", Lock, Ct);

        Assert.Equal(IdempotencyBeginStatus.Mismatch, other.Status);
    }

    [Fact]
    public async Task ReleasedKeyCanBeClaimedAgain()
    {
        var key = NewKey();
        await _store.TryBeginAsync(key, "fp", Lock, Ct);
        await _store.ReleaseAsync(key, Ct);

        var retry = await _store.TryBeginAsync(key, "fp", Lock, Ct);

        Assert.Equal(IdempotencyBeginStatus.Started, retry.Status);
    }

    [Fact]
    public async Task StaleInProgressLockExpiresSoACrashedRequestDoesNotBlockForever()
    {
        var key = NewKey();
        await _store.TryBeginAsync(key, "fp", TimeSpan.FromMilliseconds(500), Ct);

        await Task.Delay(TimeSpan.FromSeconds(1.5), Ct);
        var retry = await _store.TryBeginAsync(key, "fp", Lock, Ct);

        Assert.Equal(IdempotencyBeginStatus.Started, retry.Status);
    }

    [Fact]
    public async Task ConcurrentClaimsLetExactlyOneRequestStart()
    {
        var key = NewKey();

        var results = await Task.WhenAll(Enumerable.Range(0, 20)
            .Select(_ => _store.TryBeginAsync(key, "fp", Lock, Ct)));

        Assert.Single(results, r => r.Status == IdempotencyBeginStatus.Started);
        Assert.Equal(19, results.Count(r => r.Status == IdempotencyBeginStatus.InProgress));
    }
}
