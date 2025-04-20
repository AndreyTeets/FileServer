using System.Text.Json;

namespace FileServer.Configuration;

internal static class StaticSettings
{
    public static JsonSerializerOptions JsonOptions => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static CookieOptions GetAuthTokenCookieOptions(string domain) => new()
    {
        Secure = true,
        HttpOnly = true,
        IsEssential = true,
        Domain = domain,
        SameSite = SameSiteMode.Strict,
    };
}
