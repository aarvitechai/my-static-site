namespace RedStarVibe.Api.Infrastructure;

/// <summary>
/// Connection strings are read when a service is first created (not while registering services), so tests and
/// other hosts can override configuration up to the last moment. <see cref="EnsureRequiredConfiguration"/> keeps
/// the fail-fast behaviour: the API refuses to start, with a clear message, if one is missing.
/// </summary>
internal static class RequiredConfiguration
{
    private static readonly string[] RequiredConnectionStrings = ["Sql", "Redis"];

    public static string GetRequiredConnectionString(this IConfiguration configuration, string name) =>
        configuration.GetConnectionString(name) is { Length: > 0 } value
            ? value
            : throw new InvalidOperationException(
                $"Connection string '{name}' is not configured. See README: appsettings.Development.json (Sql) or dotnet user-secrets (Redis).");

    public static WebApplication EnsureRequiredConfiguration(this WebApplication app)
    {
        foreach (var name in RequiredConnectionStrings)
        {
            app.Configuration.GetRequiredConnectionString(name);
        }

        return app;
    }
}
