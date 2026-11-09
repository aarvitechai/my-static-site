using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using RedStarVibe.BuildingBlocks.Users;

namespace RedStarVibe.BuildingBlocks.Web.Users;

/// <summary>Reads the current user from the authenticated principal (claims are filled in by authentication in Phase 2).</summary>
internal sealed class HttpContextCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Guid? UserId
    {
        get
        {
            var principal = accessor.HttpContext?.User;
            if (principal?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            var value = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub");
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public bool IsAuthenticated => UserId is not null;
}
