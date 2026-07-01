using _10Authentication.Users;

namespace _10Authentication.Tokens;

public interface IDemoTokenService
{
    Task<TokenIssueResult> IssueAsync(DemoUser user, CancellationToken cancellationToken = default);

    Task<TokenValidationResult> ValidateAsync(string token, CancellationToken cancellationToken = default);
}
