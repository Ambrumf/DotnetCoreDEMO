using System.Text.Json.Serialization;

namespace _10Authentication.Tokens;

public sealed class DemoTokenPayload
{
    [JsonPropertyName("jti")]
    public string TokenId { get; init; } = string.Empty;

    [JsonPropertyName("sub")]
    public string Subject { get; init; } = string.Empty;

    [JsonPropertyName("username")]
    public string UserName { get; init; } = string.Empty;

    [JsonPropertyName("display_name")]
    public string DisplayName { get; init; } = string.Empty;

    [JsonPropertyName("roles")]
    public string[] Roles { get; init; } = [];

    [JsonPropertyName("permissions")]
    public string[] Permissions { get; init; } = [];

    [JsonPropertyName("iat")]
    public long IssuedAtUnixTime { get; init; }

    [JsonPropertyName("exp")]
    public long ExpiresAtUnixTime { get; init; }

    [JsonPropertyName("iss")]
    public string Issuer { get; init; } = string.Empty;

    [JsonPropertyName("aud")]
    public string Audience { get; init; } = string.Empty;

    [JsonPropertyName("security_stamp")]
    public string SecurityStamp { get; init; } = string.Empty;
}
