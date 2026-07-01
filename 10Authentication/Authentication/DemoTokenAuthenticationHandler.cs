using System.Text.Encodings.Web;
using _10Authentication.Tokens;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace _10Authentication.Authentication;

public sealed class DemoTokenAuthenticationHandler : AuthenticationHandler<DemoTokenAuthenticationOptions>
{
    private const string FailureItemKey = "__demo_auth_failure";
    private readonly IDemoTokenService _tokenService;

    public DemoTokenAuthenticationHandler(
        IOptionsMonitor<DemoTokenAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IDemoTokenService tokenService)
        : base(options, logger, encoder)
    {
        _tokenService = tokenService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(Options.AuthorizationHeaderName, out StringValues values))
        {
            return AuthenticateResult.NoResult();
        }

        string authorization = values.ToString();
        if (string.IsNullOrWhiteSpace(authorization))
        {
            return Fail("Authorization header is empty.");
        }

        string expectedPrefix = Options.TokenPrefix + " ";
        if (!authorization.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return Fail($"Authorization header must use '{Options.TokenPrefix} <token>'.");
        }

        string token = authorization[expectedPrefix.Length..].Trim();
        if (string.IsNullOrWhiteSpace(token))
        {
            return Fail("Token is empty.");
        }

        TokenValidationResult result = await _tokenService.ValidateAsync(token, Context.RequestAborted);
        if (!result.Succeeded || result.Principal is null)
        {
            return Fail(result.FailureMessage ?? "Token validation failed.");
        }

        AuthenticationTicket ticket = new(result.Principal, Scheme.Name);
        return AuthenticateResult.Success(ticket);
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.Headers.WWWAuthenticate = $"{Options.TokenPrefix} realm=\"{Options.Realm}\"";

        ProblemDetails problem = new()
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Authentication required",
            Detail = Options.IncludeFailureDetails
                ? Context.Items[FailureItemKey] as string ?? "Missing or invalid Demo token."
                : "Missing or invalid Demo token."
        };

        await Response.WriteAsJsonAsync(problem, Context.RequestAborted);
    }

    protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;

        ProblemDetails problem = new()
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Forbidden",
            Detail = "The token is valid, but it does not contain the required role or permission."
        };

        await Response.WriteAsJsonAsync(problem, Context.RequestAborted);
    }

    private AuthenticateResult Fail(string message)
    {
        Logger.LogWarning("Demo authentication failed: {Message}", message);
        Context.Items[FailureItemKey] = message;
        return AuthenticateResult.Fail(message);
    }
}
