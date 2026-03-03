using System.Security.Claims;

namespace backend.Modules.Shared.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetRequiredUserId(this ClaimsPrincipal principal)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            throw new UnauthorizedAccessException("Authenticated user id claim is missing.");

        if (!Guid.TryParse(userId, out var parsedUserId))
            throw new UnauthorizedAccessException("Authenticated user id claim is invalid.");

        return parsedUserId;
    }
}
