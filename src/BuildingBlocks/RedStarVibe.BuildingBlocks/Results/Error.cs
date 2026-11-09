namespace RedStarVibe.BuildingBlocks.Results;

/// <summary>What kind of failure this is. The web layer maps each kind to an HTTP status code.</summary>
public enum ErrorType
{
    Failure,
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden,
}

/// <summary>
/// An expected, named failure (e.g. "identity.email_taken"). Codes are stable so clients can react to them;
/// messages are for humans and must never contain internal details (SQL, stack traces, other users' data).
/// </summary>
public sealed record Error(string Code, string Message, ErrorType Type)
{
    public static Error Failure(string code, string message) => new(code, message, ErrorType.Failure);

    public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);

    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);

    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);

    public static Error Unauthorized(string code, string message) => new(code, message, ErrorType.Unauthorized);

    public static Error Forbidden(string code, string message) => new(code, message, ErrorType.Forbidden);
}
