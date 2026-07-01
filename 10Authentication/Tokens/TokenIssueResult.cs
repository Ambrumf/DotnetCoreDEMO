namespace _10Authentication.Tokens;

public sealed record TokenIssueResult(
    string AccessToken,
    string TokenType,
    DateTimeOffset ExpiresAt,
    string TokenId);
