namespace RedStarVibe.BuildingBlocks.Web.Idempotency;

/// <summary>A stored response that can be replayed for a repeated request.</summary>
public sealed record IdempotentResponse(int StatusCode, string? ContentType, string? Location, byte[] Body);

public enum IdempotencyBeginStatus
{
    /// <summary>First time this key is seen: the caller must process the request, then Complete or Release.</summary>
    Started,

    /// <summary>The same request already finished: replay the stored response.</summary>
    Completed,

    /// <summary>The same request is being processed right now (e.g. a double click).</summary>
    InProgress,

    /// <summary>The key was used before with a different request body or URL.</summary>
    Mismatch,
}

public sealed record IdempotencyBeginResult(IdempotencyBeginStatus Status, IdempotentResponse? Response = null);

/// <summary>Where idempotency keys and their responses are kept. Must support an atomic "claim if absent".</summary>
public interface IIdempotencyStore
{
    Task<IdempotencyBeginResult> TryBeginAsync(string key, string fingerprint, TimeSpan lockTtl, CancellationToken cancellationToken);

    Task CompleteAsync(string key, string fingerprint, IdempotentResponse response, TimeSpan ttl, CancellationToken cancellationToken);

    /// <summary>Forget the key (the request failed with a server error), so the client can safely retry.</summary>
    Task ReleaseAsync(string key, CancellationToken cancellationToken);
}
