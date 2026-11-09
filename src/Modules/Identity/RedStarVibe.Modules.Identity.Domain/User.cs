using Microsoft.AspNetCore.Identity;

namespace RedStarVibe.Modules.Identity.Domain;

/// <summary>
/// An account. Guid keys (ADR-0003) don't reveal how many users exist and can't be guessed in sequence,
/// unlike auto-increment integers.
/// </summary>
public sealed class User : IdentityUser<Guid>
{
    /// <summary>When the account was created, in UTC. Set from TimeProvider, never DateTime.Now.</summary>
    public DateTimeOffset CreatedAt { get; set; }
}
