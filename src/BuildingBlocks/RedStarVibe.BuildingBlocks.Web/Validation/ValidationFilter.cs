using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace RedStarVibe.BuildingBlocks.Web.Validation;

/// <summary>
/// Runs the FluentValidation validator for <typeparamref name="TRequest"/> before the endpoint handler.
/// Invalid input never reaches business logic; the client gets a 400 with an error list per field.
/// </summary>
internal sealed class ValidationFilter<TRequest>(IValidator<TRequest> validator) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<TRequest>().FirstOrDefault();
        if (request is null)
        {
            return TypedResults.Problem(statusCode: StatusCodes.Status400BadRequest, title: "A request body is required.");
        }

        var result = await validator.ValidateAsync(request, context.HttpContext.RequestAborted);
        if (!result.IsValid)
        {
            return TypedResults.ValidationProblem(result.ToDictionary());
        }

        return await next(context);
    }
}

public static class ValidationEndpointExtensions
{
    /// <summary>Validate the <typeparamref name="TRequest"/> argument with its registered <see cref="IValidator{T}"/>.</summary>
    public static RouteHandlerBuilder WithValidation<TRequest>(this RouteHandlerBuilder builder) =>
        builder
            .AddEndpointFilter<ValidationFilter<TRequest>>()
            .ProducesValidationProblem();
}
