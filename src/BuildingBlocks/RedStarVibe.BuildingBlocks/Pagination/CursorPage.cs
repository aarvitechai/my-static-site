namespace RedStarVibe.BuildingBlocks.Pagination;

/// <summary>One page of results plus the cursor for the next page (null on the last page).</summary>
public sealed record CursorPage<T>(IReadOnlyList<T> Items, string? NextCursor);

public static class CursorPagination
{
    public const int DefaultLimit = 20;
    public const int MaxLimit = 100;

    /// <summary>Clamp a client-supplied page size so nobody can request a million rows in one call.</summary>
    public static int NormalizeLimit(int? requested) =>
        requested is null or <= 0 ? DefaultLimit : Math.Min(requested.Value, MaxLimit);

    /// <summary>
    /// Build a page from a query that fetched <c>limit + 1</c> rows. The extra row only tells us whether
    /// another page exists; it isn't returned. This avoids a separate (slow) COUNT query.
    /// </summary>
    public static CursorPage<T> FromLookahead<T>(IReadOnlyList<T> rows, int limit, Func<T, Cursor> cursorOf)
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(cursorOf);
        ArgumentOutOfRangeException.ThrowIfLessThan(limit, 1);

        if (rows.Count <= limit)
        {
            return new CursorPage<T>(rows, null);
        }

        var items = rows.Take(limit).ToList();
        return new CursorPage<T>(items, cursorOf(items[^1]).Encode());
    }
}
