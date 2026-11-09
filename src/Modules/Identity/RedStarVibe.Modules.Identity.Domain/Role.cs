using Microsoft.AspNetCore.Identity;

namespace RedStarVibe.Modules.Identity.Domain;

public sealed class Role : IdentityRole<Guid>
{
}

/// <summary>
/// The platform roles from plan.md §0. Use these constants in authorization policies instead of
/// string literals, so a typo is a compile error rather than a silent security hole.
/// </summary>
public static class Roles
{
    public const string User = "User";
    public const string Vendor = "Vendor";
    public const string BusinessOwner = "BusinessOwner";
    public const string Moderator = "Moderator";
    public const string Admin = "Admin";

    public static IReadOnlyList<string> All { get; } = [User, Vendor, BusinessOwner, Moderator, Admin];
}
