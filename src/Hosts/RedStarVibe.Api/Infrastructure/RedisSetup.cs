using StackExchange.Redis;

namespace RedStarVibe.Api.Infrastructure;

internal static class RedisSetup
{
    /// <summary>
    /// One shared Redis connection for the whole process. ConnectionMultiplexer is thread-safe and designed to be
    /// reused; creating one per request would exhaust sockets. abortConnect=false (in the connection string) lets the
    /// app start while Redis is down; /health/ready reports it instead.
    /// </summary>
    public static IServiceCollection AddApiRedis(this IServiceCollection services)
    {
        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(sp.GetRequiredService<IConfiguration>().GetRequiredConnectionString("Redis")));
        return services;
    }
}
