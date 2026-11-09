using Microsoft.AspNetCore.Http;
using RedStarVibe.BuildingBlocks.Results;

namespace RedStarVibe.BuildingBlocks.Web.Results;

/// <summary>Turns a <see cref="Result"/> into an HTTP response, so every module maps failures the same way.</summary>
public static class ResultHttpExtensions
{
    /// <summary>Success -> whatever <paramref name="onSuccess"/> returns; failure -> ProblemDetails with the error code.</summary>
    public static IResult Match<T>(this Result<T> result, Func<T, IResult> onSuccess)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(onSuccess);
        return result.IsSuccess ? onSuccess(result.Value) : result.Error.ToProblem();
    }

    public static IResult Match(this Result result, Func<IResult> onSuccess)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(onSuccess);
        return result.IsSuccess ? onSuccess() : result.Error.ToProblem();
    }

    public static IResult ToProblem(this Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        var (status, title) = error.Type switch
        {
            ErrorType.Validation => (StatusCodes.Status400BadRequest, "Invalid request"),
            ErrorType.Unauthorized => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            ErrorType.Forbidden => (StatusCodes.Status403Forbidden, "Forbidden"),
            ErrorType.NotFound => (StatusCodes.Status404NotFound, "Not found"),
            ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (StatusCodes.Status422UnprocessableEntity, "Request could not be completed"),
        };

        // "code" is stable and machine-readable; clients switch on it, never on the human-readable detail.
        return TypedResults.Problem(
            detail: error.Message,
            statusCode: status,
            title: title,
            extensions: new Dictionary<string, object?> { ["code"] = error.Code });
    }
}
