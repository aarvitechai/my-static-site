using System.Buffers.Binary;
using System.Buffers.Text;

namespace RedStarVibe.BuildingBlocks.Pagination;

/// <summary>
/// Position in a list ordered by (CreatedAt DESC, Id DESC), the order every feed and listing uses
/// (index: CreatedAt DESC, Id DESC). Keyset ("cursor") pagination stays fast on huge tables and never
/// skips or repeats rows when new items arrive, unlike OFFSET/page numbers.
/// </summary>
public readonly record struct Cursor(DateTimeOffset CreatedAt, Guid Id)
{
    private const byte Version = 1;
    private const int Length = 1 + sizeof(long) + 16;

    /// <summary>Opaque, URL-safe text. Clients must treat it as a token, not parse it.</summary>
    public string Encode()
    {
        Span<byte> bytes = stackalloc byte[Length];
        bytes[0] = Version;
        BinaryPrimitives.WriteInt64BigEndian(bytes[1..], CreatedAt.UtcTicks);
        Id.TryWriteBytes(bytes[9..]);
        return Base64Url.EncodeToString(bytes);
    }

    /// <summary>
    /// Parses a cursor from a client. Returns false for anything malformed, so a bad cursor becomes a 400,
    /// never an exception. Note: a cursor only says where to continue; access rules are always applied in the query.
    /// </summary>
    public static bool TryDecode(string? text, out Cursor cursor)
    {
        cursor = default;
        // Check the alphabet first: Base64Url.TryDecodeFromChars throws (rather than returning false) on
        // characters outside it, which would turn a garbage cursor from a client into a 500.
        if (string.IsNullOrEmpty(text) || text.Length > 64 || !text.All(IsBase64UrlChar))
        {
            return false;
        }

        Span<byte> bytes = stackalloc byte[48];
        if (!Base64Url.TryDecodeFromChars(text, bytes, out var written) || written != Length || bytes[0] != Version)
        {
            return false;
        }

        var ticks = BinaryPrimitives.ReadInt64BigEndian(bytes[1..]);
        if (ticks < DateTimeOffset.MinValue.UtcTicks || ticks > DateTimeOffset.MaxValue.UtcTicks)
        {
            return false;
        }

        cursor = new Cursor(new DateTimeOffset(ticks, TimeSpan.Zero), new Guid(bytes[9..Length]));
        return true;
    }

    private static bool IsBase64UrlChar(char c) => char.IsAsciiLetterOrDigit(c) || c is '-' or '_';
}
