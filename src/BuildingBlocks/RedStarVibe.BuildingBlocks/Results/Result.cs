using System.Diagnostics.CodeAnalysis;

namespace RedStarVibe.BuildingBlocks.Results;

/// <summary>
/// The outcome of an operation that can fail in an expected way. Use it instead of throwing for
/// business failures ("email already registered", "date already booked"): the failure becomes part of the
/// method signature, callers must handle it, and exceptions stay reserved for real bugs and outages.
/// </summary>
public class Result
{
    private protected Result(Error? error) => Error = error;

    public Error? Error { get; }

    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess => Error is null;

    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsFailure => !IsSuccess;

    public static Result Success() => new(null);

    public static Result Failure(Error error) => new(error ?? throw new ArgumentNullException(nameof(error)));

    /// <summary><c>Result.Success(user)</c>: the type argument is inferred.</summary>
    public static Result<T> Success<T>(T value) => new(value);

    public static Result<T> Failure<T>(Error error) => new(error ?? throw new ArgumentNullException(nameof(error)));

    public static implicit operator Result(Error error) => Failure(error);
}

/// <summary>A <see cref="Result"/> that carries a value when it succeeds.</summary>
public sealed class Result<T> : Result
{
    private readonly T? _value;

    internal Result(T value) : base(null) => _value = value;

    internal Result(Error error) : base(error) => _value = default;

    /// <summary>The value. Reading it from a failed result is a programming error, so it throws.</summary>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException($"Cannot read the value of a failed result ({Error.Code}).");

    // Lets methods simply `return user;` or `return IdentityErrors.EmailTaken;`.
    public static implicit operator Result<T>(T value) => new(value);

    public static implicit operator Result<T>(Error error) => new(error ?? throw new ArgumentNullException(nameof(error)));
}
