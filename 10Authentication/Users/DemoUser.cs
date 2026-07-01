namespace _10Authentication.Users;

public sealed class DemoUser
{
    public required string Id { get; init; }

    public required string UserName { get; init; }

    public required string DisplayName { get; init; }

    public required string PasswordHash { get; init; }

    public required string SecurityStamp { get; init; }

    public bool IsActive { get; init; } = true;

    public IReadOnlyCollection<string> Roles { get; init; } = [];

    public IReadOnlyCollection<string> Permissions { get; init; } = [];
}
