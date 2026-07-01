using _10Authentication.Authentication;
using _10Authentication.Contracts;
using _10Authentication.Tokens;
using _10Authentication.Users;
using Microsoft.AspNetCore.Http.HttpResults;

namespace _10Authentication.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/auth");

        group.MapGet("/demo-accounts", (IUserStore users) =>
        {
            return Results.Ok(users.GetDemoAccounts());
        }).AllowAnonymous();

        group.MapPost("/login", async Task<Results<Ok<LoginResponse>, UnauthorizedHttpResult, ProblemHttpResult, ValidationProblem>> (
            LoginRequest request,
            IUserStore users,
            IDemoTokenService tokenService,
            CancellationToken cancellationToken) =>
        {
            Dictionary<string, string[]> errors = ValidateLoginRequest(request);
            if (errors.Count > 0)
            {
                return TypedResults.ValidationProblem(errors);
            }

            DemoUser? user = await users.FindByUserNameAsync(request.UserName, cancellationToken);
            if (user is null || !users.VerifyPassword(user, request.Password))
            {
                return TypedResults.Unauthorized();
            }

            if (!user.IsActive)
            {
                return TypedResults.Problem(
                    title: "User is disabled",
                    detail: "The user exists, but it is not allowed to sign in.",
                    statusCode: StatusCodes.Status403Forbidden);
            }

            TokenIssueResult token = await tokenService.IssueAsync(user, cancellationToken);
            LoginResponse response = new(
                token.AccessToken,
                token.TokenType,
                token.ExpiresAt,
                token.TokenId,
                $"{token.TokenType} {token.AccessToken}");

            return TypedResults.Ok(response);
        }).AllowAnonymous();

        group.MapGet("/me", (HttpContext context) =>
        {
            ProfileResponse response = CreateProfileResponse(context);
            return Results.Ok(response);
        }).RequireAuthorization();

        group.MapPost("/logout", async (HttpContext context, IRevokedTokenStore revokedTokens, CancellationToken cancellationToken) =>
        {
            string? tokenId = context.User.ClaimValue(DemoClaimTypes.TokenId);
            string? expiresAtUnixTime = context.User.ClaimValue(DemoClaimTypes.ExpiresAtUnixTime);

            if (string.IsNullOrWhiteSpace(tokenId) || !long.TryParse(expiresAtUnixTime, out long expiresAtSeconds))
            {
                return Results.Problem(
                    title: "Token metadata is missing",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            DateTimeOffset expiresAt = DateTimeOffset.FromUnixTimeSeconds(expiresAtSeconds);
            await revokedTokens.RevokeAsync(tokenId, expiresAt, cancellationToken);

            return Results.Ok(new TokenRevokedResponse("Token revoked.", tokenId, expiresAt));
        }).RequireAuthorization("CanRevokeToken");

        return app;
    }

    private static Dictionary<string, string[]> ValidateLoginRequest(LoginRequest request)
    {
        Dictionary<string, string[]> errors = [];

        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            errors[nameof(request.UserName)] = ["UserName is required."];
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            errors[nameof(request.Password)] = ["Password is required."];
        }

        return errors;
    }

    private static ProfileResponse CreateProfileResponse(HttpContext context)
    {
        return new ProfileResponse(
            context.User.ClaimValue(System.Security.Claims.ClaimTypes.NameIdentifier) ?? string.Empty,
            context.User.Identity?.Name ?? string.Empty,
            context.User.ClaimValue(DemoClaimTypes.DisplayName),
            context.User.ClaimValues(System.Security.Claims.ClaimTypes.Role),
            context.User.ClaimValues(DemoClaimTypes.Permission),
            context.User.ClaimValue(DemoClaimTypes.TokenId),
            context.User.ClaimValue(DemoClaimTypes.ExpiresAtUnixTime));
    }
}
