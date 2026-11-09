using Scalar.AspNetCore;

namespace RedStarVibe.Api.OpenApi;

internal static class OpenApiExtensions
{
    public static IServiceCollection AddApiDocs(this IServiceCollection services)
    {
        services.AddOpenApi("v1", options => options.AddDocumentTransformer((document, _, _) =>
        {
            document.Info.Title = "Red Star Vibe API";
            document.Info.Version = "v1";
            document.Info.Description = "Social, events, weddings, vendors and local businesses. All endpoints live under /api/v1.";
            return Task.CompletedTask;
        }));

        return services;
    }

    /// <summary>
    /// Development only: the OpenAPI document (/openapi/v1.json) and the Scalar explorer (/scalar).
    /// Not exposed in production, so the full API surface isn't handed to scanners and attackers.
    /// </summary>
    public static WebApplication MapApiDocs(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return app;
        }

        app.MapOpenApi();
        app.MapScalarApiReference(options => options.WithTitle("Red Star Vibe API"));
        return app;
    }
}
