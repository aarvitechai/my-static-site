namespace RedStarVibe.BuildingBlocks.Users;

/// <summary>
/// Who is making the current request. Application code asks this instead of reading HTTP claims directly,
/// so business logic stays testable and doesn't depend on ASP.NET Core.
/// </summary>
public interface ICurrentUser
{
    /// <summary>The signed-in user's ID, or null for anonymous requests.</summary>
    Guid? UserId { get; }

    bool IsAuthenticated { get; }
}
