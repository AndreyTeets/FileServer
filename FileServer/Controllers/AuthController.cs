﻿using FileServer.Configuration;
using FileServer.Models;
using FileServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FileServer.Controllers;

[Route("api/auth")]
public class AuthController(
    IOptionsMonitor<Settings> options,
    TokenService tokenService)
    : ControllerBase
{
    private readonly IOptionsMonitor<Settings> _options = options;
    private readonly TokenService _tokenService = tokenService;

    [HttpPost("login")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [AllowAnonymous]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid || request.Password != _options.CurrentValue.LoginKey)
            return BadRequest("Invalid password.");

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
        return new LoginResponse()
        {
            LoginInfo = new()
            {
                User = user,
                TokensExpire = tokensExpire,
            },
            AntiforgeryToken = _tokenService.EncodeToken(antiforgeryToken),
        };
    }

    [HttpPost("logout")]
    [Produces("application/json")]
    public ActionResult Logout()
    {
        DeleteAuthTokenCookie();
        return Ok();
    }

    private void SetAuthTokenCookie(Token token)
    {
        CookieOptions cookieOptions = StaticSettings.GetAuthTokenCookieOptions(Request.Host.Host);
        cookieOptions.Expires = token.Claim!.Expires;
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
