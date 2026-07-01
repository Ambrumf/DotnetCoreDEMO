using Microsoft.AspNetCore.Authentication;

namespace _10Authentication.Authentication;

public sealed class DemoTokenAuthenticationOptions : AuthenticationSchemeOptions
{
    public string AuthorizationHeaderName { get; set; } = "Authorization";

    public string TokenPrefix { get; set; } = DemoTokenAuthenticationDefaults.TokenPrefix;

    public string Realm { get; set; } = "10Authentication";

    public bool IncludeFailureDetails { get; set; }
}
