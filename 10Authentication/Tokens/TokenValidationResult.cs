using System.Security.Claims;

namespace _10Authentication.Tokens;

public sealed class TokenValidationResult
{
    private TokenValidationResult(
        bool succeeded,
        ClaimsPrincipal? principal,
        DemoTokenPayload? payload,
        string? failureMessage)
    {
        Succeeded = succeeded;
        Principal = principal;
        Payload = payload;
        FailureMessage = failureMessage;
    }

    public bool Succeeded { get; }

    public ClaimsPrincipal? Principal { get; }

    public DemoTokenPayload? Payload { get; }

    public string? FailureMessage { get; }

    public static TokenValidationResult Success(ClaimsPrincipal principal, DemoTokenPayload payload)
    {
        return new TokenValidationResult(true, principal, payload, null);
    }

    public static TokenValidationResult Fail(string message)
    {
        return new TokenValidationResult(false, null, null, message);
    }
}
