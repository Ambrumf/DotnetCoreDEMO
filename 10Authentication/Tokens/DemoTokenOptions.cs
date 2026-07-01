namespace _10Authentication.Tokens;

public sealed class DemoTokenOptions
{
    public const string SectionName = "DemoToken";

    public string Issuer { get; set; } = "10Authentication";

    public string Audience { get; set; } = "10AuthenticationStudents";

    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenLifetimeMinutes { get; set; } = 30;

    public int ClockSkewSeconds { get; set; } = 60;
}
