using System.Security.Claims;

namespace FileServer.Routes;

internal abstract class RouteHandlerBase(
    IHttpContextAccessor httpContextAccessor)
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    protected HttpContext Context => _httpContextAccessor.HttpContext!;
    protected HttpRequest Request => Context.Request;
    protected HttpResponse Response => Context.Response;
    protected CancellationToken Ct => Context.RequestAborted;

    protected bool IsAuthenticated()
    {
        ClaimsPrincipal user = Context.User;
        return user.Identities.Any(x =>
            x.IsAuthenticated &&
            x.AuthenticationType == Constants.DoubleTokenAuthenticationSchemeAuthenticationType &&
            x.Name == Constants.MainUserName);
    }
}
