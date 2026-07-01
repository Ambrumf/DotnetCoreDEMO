namespace _10Authentication.Users;

public sealed record DemoAccountInfo(
    string UserName,
    string Password,
    bool IsActive,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions);
