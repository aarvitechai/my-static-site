using RedStarVibe.BuildingBlocks.Pagination;

namespace RedStarVibe.BuildingBlocks.Tests;

public sealed class CursorTests
{
    [Fact]
    public void EncodeThenDecodeRoundTrips()
    {
        var original = new Cursor(new DateTimeOffset(2026, 11, 9, 10, 30, 15, 123, TimeSpan.Zero), Guid.NewGuid());

        var text = original.Encode();

        Assert.True(Cursor.TryDecode(text, out var decoded));
        Assert.Equal(original, decoded);
        Assert.DoesNotContain('+', text);   // URL-safe alphabet
        Assert.DoesNotContain('/', text);
        Assert.DoesNotContain('=', text);
    }

    [Fact]
    public void NonUtcTimesAreStoredAsTheSameInstant()
    {
        var local = new DateTimeOffset(2026, 11, 9, 16, 0, 0, TimeSpan.FromHours(5.5));

        Assert.True(Cursor.TryDecode(new Cursor(local, Guid.Empty).Encode(), out var decoded));
        Assert.Equal(local.UtcDateTime, decoded.CreatedAt.UtcDateTime);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("!!!not-base64!!!")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]   // too long
    public void MalformedCursorsAreRejected(string? text) =>
        Assert.False(Cursor.TryDecode(text, out _));

    [Fact]
    public void UnknownVersionIsRejected()
    {
        // A well-formed cursor body, but with version byte 2 instead of 1.
        var bytes = new byte[25];
        bytes[0] = 2;
        var otherVersion = System.Buffers.Text.Base64Url.EncodeToString(bytes);

        Assert.False(Cursor.TryDecode(otherVersion, out _));
    }

    [Theory]
    [InlineData("AQ==")]              // padding characters aren't part of the URL-safe format
    [InlineData("AQAAAAAA AAAAAAA")]  // whitespace
    [InlineData("Aé")]                // non-ASCII
    public void CharactersOutsideTheAlphabetAreRejectedWithoutThrowing(string text) =>
        Assert.False(Cursor.TryDecode(text, out _));

    [Theory]
    [InlineData(null, 20)]
    [InlineData(0, 20)]
    [InlineData(-5, 20)]
    [InlineData(5, 5)]
    [InlineData(100, 100)]
    [InlineData(5000, 100)]
    public void LimitIsNormalized(int? requested, int expected) =>
        Assert.Equal(expected, CursorPagination.NormalizeLimit(requested));

    [Fact]
    public void LookaheadRowProducesNextCursorButIsNotReturned()
    {
        var rows = Enumerable.Range(1, 3)
            .Select(i => new Cursor(DateTimeOffset.UnixEpoch.AddMinutes(-i), Guid.NewGuid()))
            .ToList();

        var page = CursorPagination.FromLookahead(rows, limit: 2, cursorOf: c => c);

        Assert.Equal(2, page.Items.Count);
        Assert.NotNull(page.NextCursor);
        Assert.True(Cursor.TryDecode(page.NextCursor, out var next));
        Assert.Equal(rows[1], next);   // continue after the last returned item
    }

    [Fact]
    public void LastPageHasNoNextCursor()
    {
        var rows = new[] { new Cursor(DateTimeOffset.UnixEpoch, Guid.NewGuid()) };

        var page = CursorPagination.FromLookahead(rows, limit: 2, cursorOf: c => c);

        Assert.Single(page.Items);
        Assert.Null(page.NextCursor);
    }
}
