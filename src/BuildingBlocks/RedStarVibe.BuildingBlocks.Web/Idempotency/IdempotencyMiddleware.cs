using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RedStarVibe.BuildingBlocks.Users;

namespace RedStarVibe.BuildingBlocks.Web.Idempotency;

/// <summary>Endpoint metadata: this endpoint requires an Idempotency-Key header.</summary>
public sealed class RequiresIdempotencyKeyMetadata;

public sealed class IdempotencyOptions
{
    public const string HeaderName = "Idempotency-Key";

    /// <summary>How long a key stays "in progress" if the server dies mid-request.</summary>
    public TimeSpan InProgressTtl { get; set; } = TimeSpan.FromMinutes(2);

    /// <summary>How long a finished response can be replayed.</summary>
    public TimeSpan ResponseTtl { get; set; } = TimeSpan.FromHours(24);

    /// <summary>Larger responses aren't stored (the key is released instead).</summary>
    public int MaxStoredResponseBytes { get; set; } = 64 * 1024;
}

/// <summary>
/// Makes POSTs safe to retry. A client sends a unique Idempotency-Key with a create request; if the network drops
/// and it retries with the same key, it gets the original response instead of a duplicate order/booking/post.
/// </summary>
internal sealed partial class IdempotencyMiddleware(
    RequestDelegate next,
    IIdempotencyStore store,
    IOptions<IdempotencyOptions> options,
    ILogger<IdempotencyMiddleware> logger)
{
    public const string ReplayedHeader = "Idempotent-Replayed";

    [GeneratedRegex("^[A-Za-z0-9_-]{8,64}$", RegexOptions.CultureInvariant)]
    private static partial Regex ValidKey();

    public async Task InvokeAsync(HttpContext context, ICurrentUser currentUser, IProblemDetailsService problemDetails)
    {
        if (context.GetEndpoint()?.Metadata.GetMetadata<RequiresIdempotencyKeyMetadata>() is null)
        {
            await next(context);
            return;
        }

        var key = context.Request.Headers[IdempotencyOptions.HeaderName].ToString();
        if (!ValidKey().IsMatch(key))
        {
            await WriteProblemAsync(problemDetails, context, StatusCodes.Status400BadRequest,
                "Missing or invalid Idempotency-Key header.",
                "Send a unique key per operation (e.g. a UUID): 8-64 characters, letters, digits, '-' or '_'.");
            return;
        }

        // Keys are scoped per user, so one user's key can never replay another user's response.
        var scopedKey = $"{currentUser.UserId?.ToString("N") ?? "anonymous"}:{key}";
        var fingerprint = await FingerprintAsync(context.Request, context.RequestAborted);
        var settings = options.Value;

        var begin = await store.TryBeginAsync(scopedKey, fingerprint, settings.InProgressTtl, context.RequestAborted);
        switch (begin.Status)
        {
            case IdempotencyBeginStatus.Mismatch:
                await WriteProblemAsync(problemDetails, context, StatusCodes.Status422UnprocessableEntity,
                    "Idempotency-Key reused with a different request.",
                    "Each key must be used for exactly one operation. Generate a new key for a new request.");
                return;

            case IdempotencyBeginStatus.InProgress:
                context.Response.Headers.RetryAfter = "1";
                await WriteProblemAsync(problemDetails, context, StatusCodes.Status409Conflict,
                    "A request with this Idempotency-Key is still being processed.",
                    "Wait for the first request to finish, then retry with the same key.");
                return;

            case IdempotencyBeginStatus.Completed when begin.Response is not null:
                LogReplayed(logger, context.Request.Method, context.Request.Path);
                await ReplayAsync(context, begin.Response);
                return;
        }

        await ExecuteAndStoreAsync(context, scopedKey, fingerprint, settings);
    }

    private async Task ExecuteAndStoreAsync(HttpContext context, string scopedKey, string fingerprint, IdempotencyOptions settings)
    {
        // Capture the response body while still sending it to the client.
        var originalBody = context.Response.Body;
        await using var buffer = new MemoryStream();
        context.Response.Body = buffer;
        var stored = false;

        try
        {
            await next(context);

            buffer.Position = 0;
            await buffer.CopyToAsync(originalBody, context.RequestAborted);

            // Server errors are not stored: the client should be able to retry them.
            if (context.Response.StatusCode < 500 && buffer.Length <= settings.MaxStoredResponseBytes)
            {
                var response = new IdempotentResponse(
                    context.Response.StatusCode,
                    context.Response.ContentType,
                    context.Response.Headers.Location.ToString() is { Length: > 0 } location ? location : null,
                    buffer.ToArray());
                await store.CompleteAsync(scopedKey, fingerprint, response, settings.ResponseTtl, CancellationToken.None);
                stored = true;
            }
        }
        finally
        {
            context.Response.Body = originalBody;
            if (!stored)
            {
                await store.ReleaseAsync(scopedKey, CancellationToken.None);
            }
        }
    }

    private static async Task ReplayAsync(HttpContext context, IdempotentResponse response)
    {
        context.Response.StatusCode = response.StatusCode;
        context.Response.ContentType = response.ContentType;
        if (response.Location is not null)
        {
            context.Response.Headers.Location = response.Location;
        }

        context.Response.Headers[ReplayedHeader] = "true";
        await context.Response.Body.WriteAsync(response.Body, context.RequestAborted);
    }

    /// <summary>Method + URL + SHA-256 of the body: a key may only be reused for the identical request.</summary>
    private static async Task<string> FingerprintAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        request.EnableBuffering();
        using var sha = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        sha.AppendData(Encoding.UTF8.GetBytes($"{request.Method} {request.Path}{request.QueryString}\n"));

        var chunk = new byte[16 * 1024];
        int read;
        while ((read = await request.Body.ReadAsync(chunk, cancellationToken)) > 0)
        {
            sha.AppendData(chunk, 0, read);
        }

        request.Body.Position = 0;   // let the endpoint read the body normally
        return Convert.ToHexString(sha.GetHashAndReset());
    }

    private static Task WriteProblemAsync(IProblemDetailsService problemDetails, HttpContext context, int status, string title, string detail)
    {
        context.Response.StatusCode = status;
        return problemDetails.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = new ProblemDetails { Status = status, Title = title, Detail = detail },
        }).AsTask();
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Replayed stored response for idempotent {Method} {Path}")]
    private static partial void LogReplayed(ILogger logger, string method, PathString path);
}

public static class IdempotencyEndpointExtensions
{
    /// <summary>Require an Idempotency-Key header on this (POST) endpoint and replay responses for retries.</summary>
    public static TBuilder RequireIdempotencyKey<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder =>
        builder.WithMetadata(new RequiresIdempotencyKeyMetadata());
}
