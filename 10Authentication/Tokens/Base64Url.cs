namespace _10Authentication.Tokens;

public static class Base64Url
{
    public static string Encode(byte[] bytes)
    {
        return Convert
            .ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public static byte[] Decode(string value)
    {
        string base64 = value.Replace('-', '+').Replace('_', '/');

        switch (base64.Length % 4)
        {
            case 2:
                base64 += "==";
                break;
            case 3:
                base64 += "=";
                break;
            case 0:
                break;
            default:
                throw new FormatException("Invalid base64url value.");
        }

        return Convert.FromBase64String(base64);
    }
}
