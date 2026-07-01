using _10Authentication.Authentication;
using _10Authentication.Contracts;

namespace _10Authentication.Endpoints;

public static class DemoEndpoints
{
    public static IEndpointRouteBuilder MapDemoEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", () => Results.Ok(new
        {
            message = "Custom AuthenticationHandler demo",
            flow = new[]
            {
                "GET /auth/demo-accounts",
                "POST /auth/login",
                "GET /auth/me with Authorization: Demo <token>",
                "GET /secure/admin with admin token",
                "POST /auth/logout with admin token"
            }
        })).AllowAnonymous();

        RouteGroupBuilder group = app.MapGroup("/secure");

        group.MapGet("/profile", (HttpContext context) =>
        {
            ProfileResponse response = new(
                context.User.ClaimValue(System.Security.Claims.ClaimTypes.NameIdentifier) ?? string.Empty,
                context.User.Identity?.Name ?? string.Empty,
                context.User.ClaimValue(DemoClaimTypes.DisplayName),
                context.User.ClaimValues(System.Security.Claims.ClaimTypes.Role),
                context.User.ClaimValues(DemoClaimTypes.Permission),
                context.User.ClaimValue(DemoClaimTypes.TokenId),
                context.User.ClaimValue(DemoClaimTypes.ExpiresAtUnixTime));

            return Results.Ok(response);
        }).RequireAuthorization();

        group.MapGet("/admin", (HttpContext context) =>
        {
            return Results.Ok(new
            {
                message = "Only Admin role can read this.",
                user = context.User.Identity?.Name
            });
        }).RequireAuthorization("AdminOnly");

        group.MapGet("/payroll", (HttpContext context) =>
        {
            return Results.Ok(new
            {
                message = "Only users with payroll.read permission can read this.",
                user = context.User.Identity?.Name
            });
        }).RequireAuthorization("PayrollReaders");

        return app;
    }
}
