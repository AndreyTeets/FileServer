namespace FileServer.Routes;

internal interface IRouteHandler<in TRouteParams>
{
    public Task<IResult> Execute(TRouteParams routeParams);
}
