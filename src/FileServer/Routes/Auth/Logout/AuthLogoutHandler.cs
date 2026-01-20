using FileServer.Configuration;

namespace FileServer.Routes.Auth.Logout;

internal sealed class AuthLogoutHandler(
    IHttpContextAccessor httpContextAccessor)
    : RouteHandlerBase(httpContextAccessor)
    , IRouteHandler<AuthLogoutParams>
{
    public async Task<IResult> Execute(AuthLogoutParams routeParams)
    {
        DeleteAuthTokenCookie();
        return Results.Ok();
    }

    private void DeleteAuthTokenCookie()
    {
        CookieOptions cookieOptions = StaticSettings.AuthTokenCookieOptions;
        Response.Cookies.Delete(Constants.AuthTokenCookieName, cookieOptions);
    }
}
