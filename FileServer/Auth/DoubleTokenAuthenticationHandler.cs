using System.Security.Claims;
using System.Text.Encodings.Web;
using FileServer.Configuration;
using FileServer.Models;
using FileServer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace FileServer.Auth;

internal sealed class DoubleTokenAuthenticationHandler(
    IOptionsMonitor<DoubleTokenAuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    TokenService tokenService)
    : AuthenticationHandler<DoubleTokenAuthenticationSchemeOptions>(options, logger, encoder)
{
    private readonly TokenService _tokenService = tokenService;

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        Token? authToken = TryGetValidToken(Constants.AuthClaimType, out string? authTokenError);
        Token? antiforgeryToken = TryGetValidToken(Constants.AntiforgeryClaimType, out string? antiforgeryTokenError);
        if (authToken is not null && antiforgeryToken is not null
            && authToken.Claim!.User == antiforgeryToken.Claim!.User)
        {
            return Task.FromResult(AuthenticateResult.Success(CreateAuthTicket(authToken.Claim.User!)));
        }
        return Task.FromResult(AuthenticateResult.Fail(CreateAuthErrorMessage()));

        string CreateAuthErrorMessage() => string.Join(" ",
            new string?[] { authTokenError, antiforgeryTokenError }
                .Where(x => x is not null));
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        AuthenticateResult authResult = await HandleAuthenticateOnceSafeAsync();
        Response.StatusCode = 401;
        Response.Cookies.Delete(Constants.AuthTokenCookieName, StaticSettings.GetAuthTokenCookieOptions(Request.Host.Host));
        await Response.WriteAsJsonAsync("Failed to authenticate: " + authResult.Failure!.Message, Jsc.Default.String);
    }

    private Token? TryGetValidToken(string tokenType, out string? error)
    {
        error = null;
        string? encodedTokenString = TryGetTokenString(tokenType);
        if (encodedTokenString is not null)
        {
            Token? token = _tokenService.TryDecodeToken(encodedTokenString);
            if (token is not null && _tokenService.TokenIsValid(token) && token.Claim!.Type == tokenType)
                return token;
            error = $"{tokenType} token not valid.";
        }
        else
        {
            error = $"{tokenType} token absent.";
        }
        return null;
    }

    private static AuthenticationTicket CreateAuthTicket(string userName)
    {
        System.Security.Claims.Claim[] claims =
        [
            new System.Security.Claims.Claim(ClaimTypes.Name, userName),
        ];
        ClaimsIdentity identity = new(claims, Constants.DoubleTokenAuthenticationSchemeAuthenticationType);
        ClaimsPrincipal principal = new(identity);
        return new(principal, Constants.DoubleTokenAuthenticationSchemeName);
    }

    private string? TryGetTokenString(string tokenType)
    {
        if (tokenType == Constants.AuthClaimType)
            return TryGetEncodedAuthTokenString();
        if (tokenType == Constants.AntiforgeryClaimType)
            return TryGetEncodedAntiforgeryTokenString();
        throw new AuthException($"Invalid token type '{tokenType}'.");
    }

    private string? TryGetEncodedAuthTokenString()
    {
        if (Request.Cookies.ContainsKey(Constants.AuthTokenCookieName))
            return Request.Cookies[Constants.AuthTokenCookieName];
        return null;
    }

    private string? TryGetEncodedAntiforgeryTokenString()
    {
        if (Request.Headers.ContainsKey(Constants.AntiforgeryTokenHeaderName))
            return Request.Headers[Constants.AntiforgeryTokenHeaderName];
        if (Request.Query.ContainsKey(Constants.AntiforgeryTokenQueryParamName))
            return Request.Query[Constants.AntiforgeryTokenQueryParamName];
        return null;
    }
}
