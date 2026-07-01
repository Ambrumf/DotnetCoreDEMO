namespace _10Authentication.Tokens;

public interface IRevokedTokenStore
{
    Task RevokeAsync(string tokenId, DateTimeOffset expiresAt, CancellationToken cancellationToken = default);

    Task<bool> IsRevokedAsync(string tokenId, CancellationToken cancellationToken = default);
}
