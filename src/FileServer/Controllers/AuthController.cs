using FileServer.Configuration;
using FileServer.Models;
using FileServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FileServer.Controllers;

internal sealed class AuthController(
    IHttpContextAccessor httpContextAccessor,
    IOptionsMonitor<Settings> options,
    TokenService tokenService)
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IOptionsMonitor<Settings> _options = options;
    private readonly TokenService _tokenService = tokenService;

    private HttpRequest Request => _httpContextAccessor.HttpContext!.Request;
    private HttpResponse Response => _httpContextAccessor.HttpContext!.Response;

    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized, "application/json")]
    [AllowAnonymous]
    public IResult Login([FromBody] LoginRequest request)
    {
        if (request.Password != _options.CurrentValue.LoginKey)
            return Results.Unauthorized();

        const string user = Constants.MainUserName;
        DateTime tokensExpire = GetUtcNowWithoutFractionalSeconds()
            .AddSeconds(_options.CurrentValue.TokensTtlSeconds!.Value);

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

    [ProducesResponseType(StatusCodes.Status200OK)]
    public IResult Logout()
    {
        DeleteAuthTokenCookie();
        return Results.Ok();
    }

    private void SetAuthTokenCookie(Token token)
    {
        CookieOptions cookieOptions = StaticSettings.GetAuthTokenCookieOptions(Request.Host.Host);
        cookieOptions.Expires = (DateTimeOffset)token.Claim.Expires;
        Response.Cookies.Append(Constants.AuthTokenCookieName, _tokenService.EncodeToken(token), cookieOptions);
    }

    private void DeleteAuthTokenCookie()
    {
        CookieOptions cookieOptions = StaticSettings.GetAuthTokenCookieOptions(Request.Host.Host);
        Response.Cookies.Delete(Constants.AuthTokenCookieName, cookieOptions);
    }

    private static DateTime GetUtcNowWithoutFractionalSeconds()
    {
        DateTime dt = DateTime.UtcNow;
        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, DateTimeKind.Utc);
    }
}
