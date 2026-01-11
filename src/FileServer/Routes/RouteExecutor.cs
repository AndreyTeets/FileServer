using Microsoft.AspNetCore.Authorization;

namespace FileServer.Routes;

internal static class RouteExecutor
{
    public static Task<IResult> Exec<TRouteParams>(
        HttpContext ctx,
        [AsParameters] TRouteParams routeParams)
        where TRouteParams : notnull
    {
        IRouteHandler<TRouteParams> rh = ctx.RequestServices.GetRequiredService<IRouteHandler<TRouteParams>>();
        return rh.Execute(routeParams);
    }

    [AllowAnonymous]
    public static Task<IResult> ExecAnon<TRouteParams>(
        HttpContext ctx,
        [AsParameters] TRouteParams routeParams)
        where TRouteParams : notnull
    {
        return Exec(ctx, routeParams);
    }
}
