using RedStarVibe.BuildingBlocks.Results;

namespace RedStarVibe.BuildingBlocks.Tests;

public sealed class ResultTests
{
    private static readonly Error NotFound = Error.NotFound("thing.not_found", "Thing not found.");

    [Fact]
    public void SuccessCarriesValue()
    {
        var result = Result.Success(42);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Null(result.Error);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void FailureCarriesError()
    {
        var result = Result.Failure<int>(NotFound);

        Assert.True(result.IsFailure);
        Assert.Equal("thing.not_found", result.Error?.Code);
    }

    [Fact]
    public void ReadingValueOfFailedResultThrows()
    {
        Result<int> result = NotFound;

        var ex = Assert.Throws<InvalidOperationException>(() => result.Value);
        Assert.Contains("thing.not_found", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ImplicitConversionsCreateSuccessAndFailure()
    {
        Result<string> ok = "hello";
        Result<string> failed = NotFound;

        Assert.True(ok.IsSuccess);
        Assert.Equal("hello", ok.Value);
        Assert.True(failed.IsFailure);
    }

    [Fact]
    public void FailureWithoutErrorIsRejected() =>
        Assert.Throws<ArgumentNullException>(() => Result.Failure(null!));
}
