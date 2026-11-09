namespace RedStarVibe.Api.Errors;

internal static class ErrorHandlingExtensions
{
    /// <summary>Every error response (exceptions, 404, 405, validation) uses the same ProblemDetails shape.</summary>
    public static IServiceCollection AddApiErrorHandling(this IServiceCollection services)
    {
        services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
        {
            // Lets a user or support person quote one ID that finds every related log line in Seq.
            context.ProblemDetails.Extensions["correlationId"] = context.HttpContext.TraceIdentifier;
            context.ProblemDetails.Instance ??= context.HttpContext.Request.Path;
        });

        services.AddExceptionHandler<GlobalExceptionHandler>();
        return services;
    }

    public static IApplicationBuilder UseApiErrorHandling(this IApplicationBuilder app)
    {
        app.UseExceptionHandler();   // unhandled exceptions -> GlobalExceptionHandler
        app.UseStatusCodePages();    // empty 4xx/5xx responses (e.g. 404, 405) -> ProblemDetails body
        return app;
    }
}
