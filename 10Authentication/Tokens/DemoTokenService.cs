using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using _10Authentication.Authentication;
using _10Authentication.Users;
using Microsoft.Extensions.Options;

namespace _10Authentication.Tokens;

public sealed class DemoTokenService : IDemoTokenService
{
    private const string Version = "v1";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IOptionsMonitor<DemoTokenOptions> _options;
    private readonly IUserStore _userStore;
    private readonly IRevokedTokenStore _revokedTokenStore;
    private readonly TimeProvider _timeProvider;

    public DemoTokenService(
        IOptionsMonitor<DemoTokenOptions> options,
        IUserStore userStore,
        IRevokedTokenStore revokedTokenStore,
        TimeProvider timeProvider)
    {
        _options = options;
        _userStore = userStore;
        _revokedTokenStore = revokedTokenStore;
        _timeProvider = timeProvider;
    }

    public Task<TokenIssueResult> IssueAsync(DemoUser user, CancellationToken cancellationToken = default)
    {
        DemoTokenOptions options = _options.CurrentValue;
        DateTimeOffset now = _timeProvider.GetUtcNow();
        DateTimeOffset expiresAt = now.AddMinutes(options.AccessTokenLifetimeMinutes);

        DemoTokenPayload payload = new()
        {
            TokenId = Guid.NewGuid().ToString("N"),
            Subject = user.Id,
            UserName = user.UserName,
            DisplayName = user.DisplayName,
            Roles = user.Roles.ToArray(),
            Permissions = user.Permissions.ToArray(),
            IssuedAtUnixTime = now.ToUnixTimeSeconds(),
            ExpiresAtUnixTime = expiresAt.ToUnixTimeSeconds(),
            Issuer = options.Issuer,
            Audience = options.Audience,
            SecurityStamp = user.SecurityStamp
        };

        string payloadJson = JsonSerializer.Serialize(payload, JsonOptions);
        string encodedPayload = Base64Url.Encode(Encoding.UTF8.GetBytes(payloadJson));
        string signature = Base64Url.Encode(Sign($"{Version}.{encodedPayload}", options.SigningKey));
        string token = $"{Version}.{encodedPayload}.{signature}";

        TokenIssueResult result = new(
            token,
            DemoTokenAuthenticationDefaults.TokenPrefix,
            expiresAt,
            payload.TokenId);

        return Task.FromResult(result);
    }

    public async Task<TokenValidationResult> ValidateAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        DemoTokenOptions options = _options.CurrentValue;

        string[] parts = token.Split('.');
        if (parts.Length != 3)
        {
            return TokenValidationResult.Fail("Token must use format 'v1.payload.signature'.");
        }

        if (!string.Equals(parts[0], Version, StringComparison.Ordinal))
        {
            return TokenValidationResult.Fail("Unsupported token version.");
        }

        byte[] providedSignature;
        try
        {
            providedSignature = Base64Url.Decode(parts[2]);
        }
        catch (FormatException)
        {
            return TokenValidationResult.Fail("Token signature is not valid base64url.");
        }

        byte[] expectedSignature = Sign($"{parts[0]}.{parts[1]}", options.SigningKey);
        if (!CryptographicOperations.FixedTimeEquals(providedSignature, expectedSignature))
        {
            return TokenValidationResult.Fail("Token signature is invalid.");
        }

        DemoTokenPayload? payload;
        try
        {
            string payloadJson = Encoding.UTF8.GetString(Base64Url.Decode(parts[1]));
            payload = JsonSerializer.Deserialize<DemoTokenPayload>(payloadJson, JsonOptions);
        }
        catch (Exception ex) when (ex is FormatException or JsonException)
        {
            return TokenValidationResult.Fail("Token payload is invalid.");
        }

        if (payload is null)
        {
            return TokenValidationResult.Fail("Token payload is empty.");
        }

        TokenValidationResult payloadValidation = await ValidatePayloadAsync(payload, options, cancellationToken);
        if (!payloadValidation.Succeeded)
        {
            return payloadValidation;
        }

        ClaimsPrincipal principal = CreatePrincipal(payloadValidation.Payload!);
        return TokenValidationResult.Success(principal, payloadValidation.Payload!);
    }

    private async Task<TokenValidationResult> ValidatePayloadAsync(
        DemoTokenPayload payload,
        DemoTokenOptions options,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(payload.TokenId))
        {
            return TokenValidationResult.Fail("Token id is missing.");
        }

        if (string.IsNullOrWhiteSpace(payload.Subject))
        {
            return TokenValidationResult.Fail("Token subject is missing.");
        }

        if (!string.Equals(payload.Issuer, options.Issuer, StringComparison.Ordinal))
        {
            return TokenValidationResult.Fail("Token issuer is invalid.");
        }

        if (!string.Equals(payload.Audience, options.Audience, StringComparison.Ordinal))
        {
            return TokenValidationResult.Fail("Token audience is invalid.");
        }

        DateTimeOffset now = _timeProvider.GetUtcNow();
        TimeSpan clockSkew = TimeSpan.FromSeconds(options.ClockSkewSeconds);
        DateTimeOffset issuedAt = DateTimeOffset.FromUnixTimeSeconds(payload.IssuedAtUnixTime);
        DateTimeOffset expiresAt = DateTimeOffset.FromUnixTimeSeconds(payload.ExpiresAtUnixTime);

        if (issuedAt > now.Add(clockSkew))
        {
            return TokenValidationResult.Fail("Token issued-at time is in the future.");
        }

        if (expiresAt <= now.Subtract(clockSkew))
        {
            return TokenValidationResult.Fail("Token is expired.");
        }

        if (await _revokedTokenStore.IsRevokedAsync(payload.TokenId, cancellationToken))
        {
            return TokenValidationResult.Fail("Token has been revoked.");
        }

        DemoUser? user = await _userStore.FindByIdAsync(payload.Subject, cancellationToken);
        if (user is null)
        {
            return TokenValidationResult.Fail("Token user no longer exists.");
        }

        if (!user.IsActive)
        {
            return TokenValidationResult.Fail("Token user is disabled.");
        }

        if (!string.Equals(user.SecurityStamp, payload.SecurityStamp, StringComparison.Ordinal))
        {
            return TokenValidationResult.Fail("Token security stamp is no longer valid.");
        }

        DemoTokenPayload refreshedPayload = new()
        {
            TokenId = payload.TokenId,
            Subject = user.Id,
            UserName = user.UserName,
            DisplayName = user.DisplayName,
            Roles = user.Roles.ToArray(),
            Permissions = user.Permissions.ToArray(),
            IssuedAtUnixTime = payload.IssuedAtUnixTime,
            ExpiresAtUnixTime = payload.ExpiresAtUnixTime,
            Issuer = payload.Issuer,
            Audience = payload.Audience,
            SecurityStamp = payload.SecurityStamp
        };

        return TokenValidationResult.Success(new ClaimsPrincipal(), refreshedPayload);
    }

    private static ClaimsPrincipal CreatePrincipal(DemoTokenPayload payload)
    {
        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, payload.Subject),
            new(ClaimTypes.Name, payload.UserName),
            new(DemoClaimTypes.DisplayName, payload.DisplayName),
            new(DemoClaimTypes.TokenId, payload.TokenId),
            new(DemoClaimTypes.IssuedAtUnixTime, payload.IssuedAtUnixTime.ToString()),
            new(DemoClaimTypes.ExpiresAtUnixTime, payload.ExpiresAtUnixTime.ToString()),
            new(DemoClaimTypes.Issuer, payload.Issuer),
            new(DemoClaimTypes.Audience, payload.Audience),
            new(DemoClaimTypes.SecurityStamp, payload.SecurityStamp)
        ];

        claims.AddRange(payload.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(payload.Permissions.Select(permission => new Claim(DemoClaimTypes.Permission, permission)));

        ClaimsIdentity identity = new(
            claims,
            DemoTokenAuthenticationDefaults.AuthenticationScheme,
            ClaimTypes.Name,
            ClaimTypes.Role);

        return new ClaimsPrincipal(identity);
    }

    private static byte[] Sign(string data, string signingKey)
    {
        using HMACSHA256 hmac = new(Encoding.UTF8.GetBytes(signingKey));
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
    }
}
