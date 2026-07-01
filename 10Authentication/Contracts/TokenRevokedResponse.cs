namespace _10Authentication.Contracts;

public sealed record TokenRevokedResponse(
    string Message,
    string TokenId,
    DateTimeOffset ExpiresAt);
