namespace _10Authentication.Contracts;

public sealed record ProfileResponse(
    string UserId,
    string UserName,
    string? DisplayName,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions,
    string? TokenId,
    string? ExpiresAtUnixTime);
