using System.Threading.RateLimiting;
using FileServer.Routes.Auth.Login;

namespace FileServer.Configuration;

internal static class StaticSettings
{
    public static CookieOptions GetAuthTokenCookieOptions(string domain) => new()
    {
        Secure = true,
        HttpOnly = true,
        IsEssential = true,
        Domain = domain,
        SameSite = SameSiteMode.Strict,
    };

    public static TokenBucketRateLimiterOptions GetRateLimiterOptions(string route) =>
        s_routesRateLimiterOptions.TryGetValue($"{route}Meta", out RateLimiterOptions? options) ? new()
        {
            TokenLimit = Math.Max(options.MaxBurst, options.AvgRpm),
            QueueLimit = 0,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            TokensPerPeriod = options.AvgRpm,
        } : throw new ArgumentOutOfRangeException(nameof(route), $"Invalid route '{route}'.");

    private static readonly Dictionary<string, RateLimiterOptions> s_routesRateLimiterOptions = new()
    {
        { nameof(AuthLoginMeta), new(MaxBurst: 100, AvgRpm: 10) },
    };

    private sealed record RateLimiterOptions(int MaxBurst, int AvgRpm);
}
