namespace _10Authentication.Users;

public interface IUserStore
{
    ValueTask<DemoUser?> FindByUserNameAsync(string userName, CancellationToken cancellationToken = default);

    ValueTask<DemoUser?> FindByIdAsync(string userId, CancellationToken cancellationToken = default);

    bool VerifyPassword(DemoUser user, string password);

    IReadOnlyCollection<DemoAccountInfo> GetDemoAccounts();
}
