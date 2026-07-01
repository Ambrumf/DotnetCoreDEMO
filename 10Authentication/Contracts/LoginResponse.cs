namespace _10Authentication.Contracts;

public sealed record LoginResponse(
    string AccessToken,
    string TokenType,
    DateTimeOffset ExpiresAt,
    string TokenId,
    string ExampleAuthorizationHeader);
