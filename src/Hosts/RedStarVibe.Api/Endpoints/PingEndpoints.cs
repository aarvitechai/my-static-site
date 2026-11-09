using Microsoft.AspNetCore.Http.HttpResults;

namespace RedStarVibe.Api.Endpoints;

public sealed record PingResponse(string Status, DateTimeOffset ServerTimeUtc);

public static class PingEndpoints
{
    // Extension method pattern: each feature registers its own routes, keeping Program.cs short.
    public static RouteGroupBuilder MapPingEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/ping", Ping)
            .WithName("Ping")
            .WithTags("System")
            .WithSummary("Liveness smoke test: returns pong and the server time.")
            .AllowAnonymous();

        return group;
    }

    // TypedResults (not Results) make the response type part of the signature,
    // so OpenAPI (slice 2) can describe it exactly and the compiler checks it.
    private static Ok<PingResponse> Ping(TimeProvider clock) =>
        TypedResults.Ok(new PingResponse("pong", clock.GetUtcNow()));
}
