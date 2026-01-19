using Microsoft.AspNetCore.Authorization;

namespace FileServer.Routes;

internal static class RouteExecutor
{
    public static Task<IResult> Exec<TRouteParams>(
        HttpContext context,
        [AsParameters] TRouteParams routeParams)
        where TRouteParams : notnull
    {
        IRouteHandler<TRouteParams> rh = context.RequestServices.GetRequiredService<IRouteHandler<TRouteParams>>();
        return rh.Execute(routeParams);
    }

    [AllowAnonymous]
    public static Task<IResult> ExecAnon<TRouteParams>(
        HttpContext context,
        [AsParameters] TRouteParams routeParams)
        where TRouteParams : notnull
    {
        return Exec(context, routeParams);
    }
}
