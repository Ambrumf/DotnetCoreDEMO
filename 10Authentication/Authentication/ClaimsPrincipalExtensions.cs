using System.Security.Claims;

namespace _10Authentication.Authentication;

public static class ClaimsPrincipalExtensions
{
    public static string? ClaimValue(this ClaimsPrincipal principal, string type)
    {
        return principal.FindFirst(type)?.Value;
    }

    public static IReadOnlyCollection<string> ClaimValues(this ClaimsPrincipal principal, string type)
    {
        return principal
            .FindAll(type)
            .Select(claim => claim.Value)
            .ToArray();
    }
}
