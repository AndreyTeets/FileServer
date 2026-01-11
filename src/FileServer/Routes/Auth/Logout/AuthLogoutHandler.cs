using FileServer.Configuration;

namespace FileServer.Routes.Auth.Logout;

internal sealed class AuthLogoutHandler(
    IHttpContextAccessor httpContextAccessor)
    : IRouteHandler<AuthLogoutParams>
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    private HttpRequest Request => _httpContextAccessor.HttpContext!.Request;
    private HttpResponse Response => _httpContextAccessor.HttpContext!.Response;

    public async Task<IResult> Execute(AuthLogoutParams routeParams)
    {
        DeleteAuthTokenCookie();
        return Results.Ok();
    }

    private void DeleteAuthTokenCookie()
    {
        CookieOptions cookieOptions = StaticSettings.GetAuthTokenCookieOptions(Request.Host.Host);
        Response.Cookies.Delete(Constants.AuthTokenCookieName, cookieOptions);
    }
}
