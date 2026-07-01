using System.Text;
using _10Authentication.Tokens;
using _10Authentication.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace _10Authentication.Authentication;

public static class AuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddDemoAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<DemoTokenOptions>()
            .Bind(configuration.GetSection(DemoTokenOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "DemoToken:Issuer is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "DemoToken:Audience is required.")
            .Validate(
                options => Encoding.UTF8.GetByteCount(options.SigningKey) >= 32,
                "DemoToken:SigningKey must be at least 32 bytes.")
            .Validate(options => options.AccessTokenLifetimeMinutes > 0, "Token lifetime must be positive.")
            .ValidateOnStart();

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IUserStore, InMemoryUserStore>();
        services.AddSingleton<IRevokedTokenStore, InMemoryRevokedTokenStore>();
        services.AddSingleton<IDemoTokenService, DemoTokenService>();

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = DemoTokenAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = DemoTokenAuthenticationDefaults.AuthenticationScheme;
                options.DefaultForbidScheme = DemoTokenAuthenticationDefaults.AuthenticationScheme;
            })
            .AddScheme<DemoTokenAuthenticationOptions, DemoTokenAuthenticationHandler>(
                DemoTokenAuthenticationDefaults.AuthenticationScheme,
                options =>
                {
                    options.TokenPrefix = DemoTokenAuthenticationDefaults.TokenPrefix;
                    options.IncludeFailureDetails = true;
                });

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = null;

            options.AddPolicy("AdminOnly", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole("Admin");
            });

            options.AddPolicy("PayrollReaders", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim(DemoClaimTypes.Permission, "payroll.read");
            });

            options.AddPolicy("CanRevokeToken", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(context =>
                    context.User.IsInRole("Admin") ||
                    context.User.HasClaim(DemoClaimTypes.Permission, "token.revoke"));
            });
        });

        return services;
    }
}
