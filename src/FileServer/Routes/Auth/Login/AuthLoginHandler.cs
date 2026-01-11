using FileServer.Configuration;
using FileServer.Models.Auth;
using FileServer.Services;
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
        if (request.Password != _options.CurrentValue.LoginKey)
            return Results.Unauthorized();

        const string user = Constants.MainUserName;
        DateTime tokensExpire = GetUtcNowWithoutFractionalSeconds()
            .AddSeconds(_options.CurrentValue.TokensTtlSeconds);

        Token authToken = _tokenService.CreateToken(
            new Claim()
            {
                User = user,
                Type = Constants.AuthClaimType,
                Expires = tokensExpire,
            });
        Token antiforgeryToken = _tokenService.CreateToken(
            new Claim()
            {
                User = user,
                Type = Constants.AntiforgeryClaimType,
                Expires = tokensExpire,
            });

        SetAuthTokenCookie(authToken);
        return Results.Ok(new LoginResponse()
        {
            LoginInfo = new()
            {
                User = user,
                TokensExpire = tokensExpire,
            },
            AntiforgeryToken = _tokenService.EncodeToken(antiforgeryToken),
        });
    }

    private void SetAuthTokenCookie(Token token)
    {
        CookieOptions cookieOptions = StaticSettings.GetAuthTokenCookieOptions(Request.Host.Host);
        cookieOptions.Expires = (DateTimeOffset)token.Claim.Expires;
        Response.Cookies.Append(Constants.AuthTokenCookieName, _tokenService.EncodeToken(token), cookieOptions);
    }

    private static DateTime GetUtcNowWithoutFractionalSeconds()
    {
        DateTime dt = DateTime.UtcNow;
        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, DateTimeKind.Utc);
    }
}
