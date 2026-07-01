using System.Collections.Concurrent;

namespace _10Authentication.Tokens;

public sealed class InMemoryRevokedTokenStore : IRevokedTokenStore
{
    private readonly ConcurrentDictionary<string, DateTimeOffset> _revokedTokens = new(StringComparer.Ordinal);
    private readonly TimeProvider _timeProvider;

    public InMemoryRevokedTokenStore(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public Task RevokeAsync(string tokenId, DateTimeOffset expiresAt, CancellationToken cancellationToken = default)
    {
        RemoveExpiredTokens();
        _revokedTokens[tokenId] = expiresAt;
        return Task.CompletedTask;
    }

    public Task<bool> IsRevokedAsync(string tokenId, CancellationToken cancellationToken = default)
    {
        RemoveExpiredTokens();
        return Task.FromResult(_revokedTokens.ContainsKey(tokenId));
    }

    private void RemoveExpiredTokens()
    {
        DateTimeOffset now = _timeProvider.GetUtcNow();

        foreach ((string tokenId, DateTimeOffset expiresAt) in _revokedTokens)
        {
            if (expiresAt <= now)
            {
                _revokedTokens.TryRemove(tokenId, out _);
            }
        }
    }
}
