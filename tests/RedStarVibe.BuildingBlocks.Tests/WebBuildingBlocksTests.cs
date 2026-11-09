using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using RedStarVibe.BuildingBlocks.Results;
using RedStarVibe.BuildingBlocks.Tests.TestSupport;
using RedStarVibe.BuildingBlocks.Web.Idempotency;
using RedStarVibe.BuildingBlocks.Web.Results;
using RedStarVibe.BuildingBlocks.Web.Validation;

namespace RedStarVibe.BuildingBlocks.Tests;

public sealed class WebBuildingBlocksTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    // ---------- Validation ----------

    [Fact]
    public async Task InvalidBodyIsRejectedWithFieldErrors()
    {
        await using var app = await TestApp.StartAsync(a =>
            a.MapPost("/things", (CreateThing thing) => TypedResults.Ok(thing)).WithValidation<CreateThing>());

        var response = await app.Client.PostAsJsonAsync("/things", new CreateThing("", 99), Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Ct));
        var errors = body.RootElement.GetProperty("errors");
        Assert.True(errors.TryGetProperty("Name", out _));
        Assert.True(errors.TryGetProperty("Quantity", out _));
    }

    [Fact]
    public async Task ValidBodyReachesTheHandler()
    {
        await using var app = await TestApp.StartAsync(a =>
            a.MapPost("/things", (CreateThing thing) => TypedResults.Ok(thing)).WithValidation<CreateThing>());

        var response = await app.Client.PostAsJsonAsync("/things", new CreateThing("Kulfi", 3), Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ---------- Result -> HTTP ----------

    [Fact]
    public async Task FailedResultBecomesProblemWithStableCode()
    {
        await using var app = await TestApp.StartAsync(a => a.MapGet("/things/{id:guid}", (Guid id) =>
            Result.Failure<string>(Error.NotFound("thing.not_found", "Thing not found.")).Match(TypedResults.Ok)));

        var response = await app.Client.GetAsync($"/things/{Guid.NewGuid()}", Ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Ct));
        Assert.Equal("thing.not_found", body.RootElement.GetProperty("code").GetString());
    }

    // ---------- Idempotency ----------

    private static Task<TestApp> StartIdempotentApp(Counter counter, int statusCode = StatusCodes.Status201Created) =>
        TestApp.StartAsync(a => a.MapPost("/orders", (CreateThing thing) =>
        {
            var n = counter.Increment();
            // Fully qualified: inside RedStarVibe.BuildingBlocks.*, "Results" would resolve to our own namespace.
            return Microsoft.AspNetCore.Http.Results.Json(new { orderNumber = n, thing.Name }, statusCode: statusCode);
        }).RequireIdempotencyKey());

    private static HttpRequestMessage Post(string? key, CreateThing body)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/orders") { Content = JsonContent.Create(body) };
        if (key is not null)
        {
            request.Headers.Add(IdempotencyOptions.HeaderName, key);
        }

        return request;
    }

    [Fact]
    public async Task MissingIdempotencyKeyIsRejected()
    {
        var counter = new Counter();
        await using var app = await StartIdempotentApp(counter);

        var response = await app.Client.SendAsync(Post(null, new CreateThing("Kulfi", 1)), Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, counter.Value);
    }

    [Fact]
    public async Task RetryWithSameKeyReplaysResponseWithoutRunningTheHandlerAgain()
    {
        var counter = new Counter();
        await using var app = await StartIdempotentApp(counter);
        var key = Guid.NewGuid().ToString();

        var first = await app.Client.SendAsync(Post(key, new CreateThing("Kulfi", 1)), Ct);
        var second = await app.Client.SendAsync(Post(key, new CreateThing("Kulfi", 1)), Ct);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);
        Assert.Equal(await first.Content.ReadAsStringAsync(Ct), await second.Content.ReadAsStringAsync(Ct));
        Assert.True(second.Headers.Contains("Idempotent-Replayed"));
        Assert.Equal(1, counter.Value);   // the order was created exactly once
    }

    [Fact]
    public async Task SameKeyWithDifferentBodyIsRejected()
    {
        var counter = new Counter();
        await using var app = await StartIdempotentApp(counter);
        var key = Guid.NewGuid().ToString();

        await app.Client.SendAsync(Post(key, new CreateThing("Kulfi", 1)), Ct);
        var reused = await app.Client.SendAsync(Post(key, new CreateThing("Mango", 2)), Ct);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, reused.StatusCode);
        Assert.Equal(1, counter.Value);
    }

    [Fact]
    public async Task ConcurrentDuplicateGetsConflict()
    {
        var counter = new Counter();
        await using var app = await StartIdempotentApp(counter);
        const string key = "in-flight-key-123";

        // Send once to learn the fingerprint the middleware computes for this request,
        // then put the key back into "still processing" to simulate a double click mid-request.
        var first = await app.Client.SendAsync(Post(key, new CreateThing("Kulfi", 1)), Ct);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        app.Store.SeedInProgress("anonymous:" + key, GetStoredFingerprint(app, key));

        var duplicate = await app.Client.SendAsync(Post(key, new CreateThing("Kulfi", 1)), Ct);

        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.True(duplicate.Headers.Contains("Retry-After"));
        Assert.Equal(1, counter.Value);
    }

    [Fact]
    public async Task ServerErrorsAreNotStoredSoTheClientCanRetry()
    {
        var counter = new Counter();
        await using var app = await StartIdempotentApp(counter, statusCode: StatusCodes.Status503ServiceUnavailable);
        var key = Guid.NewGuid().ToString();

        await app.Client.SendAsync(Post(key, new CreateThing("Kulfi", 1)), Ct);
        await app.Client.SendAsync(Post(key, new CreateThing("Kulfi", 1)), Ct);

        Assert.Equal(2, counter.Value);   // retried for real, not replayed
        Assert.Equal(0, app.Store.Count);
    }

    // ---------- Rate limiting ----------

    [Fact]
    public async Task RequestsOverTheLimitGet429WithRetryAfter()
    {
        await using var app = await TestApp.StartAsync(a => a.MapGet("/things", () => "ok"), permitLimit: 2);

        var statuses = new List<HttpStatusCode>();
        HttpResponseMessage? last = null;
        for (var i = 0; i < 3; i++)
        {
            last = await app.Client.GetAsync("/things", Ct);
            statuses.Add(last.StatusCode);
        }

        Assert.Equal([HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.TooManyRequests], statuses);
        Assert.Equal("application/problem+json", last!.Content.Headers.ContentType?.MediaType);
        Assert.True(last.Headers.Contains("Retry-After"));
    }

    [Fact]
    public async Task HealthProbesAreNeverRateLimited()
    {
        await using var app = await TestApp.StartAsync(a => a.MapGet("/health/live", () => "ok"), permitLimit: 1);

        for (var i = 0; i < 5; i++)
        {
            var response = await app.Client.GetAsync("/health/live", Ct);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    private static string GetStoredFingerprint(TestApp app, string key) =>
        app.Store.FingerprintOf("anonymous:" + key) ?? throw new InvalidOperationException("No stored entry.");

    internal sealed class Counter
    {
        private int _value;

        public int Value => Volatile.Read(ref _value);

        public int Increment() => Interlocked.Increment(ref _value);
    }
}
