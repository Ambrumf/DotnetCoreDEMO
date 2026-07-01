using System.Collections.Concurrent;

namespace _10Authentication.Users;

public sealed class InMemoryUserStore : IUserStore
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly ConcurrentDictionary<string, DemoUser> _usersById = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, string> _userIdsByName = new(StringComparer.OrdinalIgnoreCase);
    private readonly DemoAccountInfo[] _demoAccounts;

    public InMemoryUserStore(IPasswordHasher passwordHasher)
    {
        _passwordHasher = passwordHasher;

        _demoAccounts =
        [
            new DemoAccountInfo(
                "admin",
                "admin123!",
                true,
                ["Admin", "User"],
                ["profile.read", "admin.read", "payroll.read", "token.revoke"]),
            new DemoAccountInfo(
                "alice",
                "alice123!",
                true,
                ["User"],
                ["profile.read"]),
            new DemoAccountInfo(
                "disabled",
                "disabled123!",
                false,
                ["User"],
                ["profile.read"])
        ];

        SeedUsers();
    }

    public ValueTask<DemoUser?> FindByUserNameAsync(
        string userName,
        CancellationToken cancellationToken = default)
    {
        if (!_userIdsByName.TryGetValue(userName, out string? userId))
        {
            return ValueTask.FromResult<DemoUser?>(null);
        }

        _usersById.TryGetValue(userId, out DemoUser? user);
        return ValueTask.FromResult(user);
    }

    public ValueTask<DemoUser?> FindByIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        _usersById.TryGetValue(userId, out DemoUser? user);
        return ValueTask.FromResult(user);
    }

    public bool VerifyPassword(DemoUser user, string password)
    {
        return _passwordHasher.Verify(password, user.PasswordHash);
    }

    public IReadOnlyCollection<DemoAccountInfo> GetDemoAccounts()
    {
        return _demoAccounts;
    }

    private void SeedUsers()
    {
        foreach (DemoAccountInfo account in _demoAccounts)
        {
            DemoUser user = new()
            {
                Id = Guid.NewGuid().ToString("N"),
                UserName = account.UserName,
                DisplayName = account.UserName.Equals("admin", StringComparison.OrdinalIgnoreCase)
                    ? "System Administrator"
                    : account.UserName,
                PasswordHash = _passwordHasher.Hash(account.Password),
                IsActive = account.IsActive,
                Roles = account.Roles.ToArray(),
                Permissions = account.Permissions.ToArray(),
                SecurityStamp = Guid.NewGuid().ToString("N")
            };

            _usersById[user.Id] = user;
            _userIdsByName[user.UserName] = user.Id;
        }
    }
}
