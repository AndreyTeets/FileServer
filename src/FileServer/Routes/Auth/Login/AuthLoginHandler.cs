using System.Security.Cryptography;
using FileServer.Configuration;
using FileServer.Models.Auth;
using FileServer.Services;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Options;

namespace FileServer.Routes.Auth.Login;

internal sealed class AuthLoginHandler(
    IHttpContextAccessor httpContextAccessor,
    IOptionsMonitor<Settings> options,
    TokenService tokenService)
    : RouteHandlerBase(httpContextAccessor)
    , IRouteHandler<AuthLoginParams>
{
    private readonly IOptionsMonitor<Settings> _options = options;
    private readonly TokenService _tokenService = tokenService;

    public async Task<IResult> Execute(AuthLoginParams routeParams)
    {
        LoginRequest request = routeParams.Request;
        string password = request.Password ?? "";

        byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);
        byte[] passwordHash = HashPassword(password, salt);
        byte[] expectedPasswordHash = HashPassword(_options.CurrentValue.LoginKey, salt);
        if (!CryptographicOperations.FixedTimeEquals(passwordHash, expectedPasswordHash))
            return Results.Unauthorized();

        const string user = Constants.MainUserName;
        DateTime tokensExpire = GetUtcNowWithoutFractionalSeconds()
            .AddSeconds(_options.CurrentValue.TokensTtlSeconds);

        SetAuthTokenCookie(_tokenService.EncodeToken(
            CreateToken(Constants.AuthClaimType, user, tokensExpire)), tokensExpire);
        return Results.Ok(new LoginResponse()
        {
            LoginInfo = new()
            {
                User = user,
                TokensExpire = tokensExpire,
            },
            AntiforgeryToken = _tokenService.EncodeToken(
                CreateToken(Constants.AntiforgeryClaimType, user, tokensExpire)),
        });
    }

    private void SetAuthTokenCookie(string token, DateTime expires)
    {
        CookieOptions cookieOptions = StaticSettings.GetAuthTokenCookieOptions(Request.Host.Host);
        cookieOptions.Expires = (DateTimeOffset)expires;
        Response.Cookies.Append(Constants.AuthTokenCookieName, token, cookieOptions);
    }

    private Token CreateToken(string type, string user, DateTime expires) =>
        _tokenService.CreateToken(new Claim()
        {
            User = user,
            Type = type,
            Expires = expires,
        });

    private static byte[] HashPassword(string password, byte[] salt) =>
        KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8);

    private static DateTime GetUtcNowWithoutFractionalSeconds()
    {
        DateTime dt = DateTime.UtcNow;
        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, DateTimeKind.Utc);
    }
}
