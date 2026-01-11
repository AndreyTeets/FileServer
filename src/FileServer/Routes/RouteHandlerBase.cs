namespace FileServer.Routes;

internal abstract class RouteHandlerBase(
    IHttpContextAccessor httpContextAccessor)
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    private protected HttpContext Context => _httpContextAccessor.HttpContext!;
    private protected HttpRequest Request => Context.Request;
    private protected HttpResponse Response => Context.Response;
    private protected CancellationToken Ct => Context.RequestAborted;
}
