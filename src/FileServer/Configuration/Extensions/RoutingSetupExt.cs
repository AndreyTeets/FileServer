using System.Reflection;
using FileServer.Routes;
using FileServer.Routes.Auth.Login;
using FileServer.Routes.Auth.Logout;
using FileServer.Routes.Files.GetFileRoutes;
using FileServer.Routes.Files.List;
using FileServer.Routes.Files.Upload;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.FileProviders;
using Route = FileServer.Routes.RouteExecutor;

namespace FileServer.Configuration.Extensions;

internal static class RoutingSetupExt
{
    public static void SetUpRouting<TApp>(this TApp app)
        where TApp : IApplicationBuilder, IEndpointRouteBuilder
    {
        app.UseToIndexPageRedirect();
        app.UseStaticFilesWithNoCacheHeaders();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapRoutes();
        app.UseNoCacheHeaders();
    }

    private static void UseToIndexPageRedirect(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            if (context.Request.Path.Value == "/")
                context.Request.Path = "/index.html";
            await next();
        });
    }

    private static void UseStaticFilesWithNoCacheHeaders(this IApplicationBuilder app)
    {
        StaticFileOptions sfo = new();
        Assembly assembly = typeof(Program).Assembly;
        if (UseEmbeddedStaticFiles(assembly))
            sfo.FileProvider = new EmbeddedFileProvider(assembly, $"{assembly.GetName().Name}.wwwroot");
        sfo.OnPrepareResponse = sfrContext =>
            sfrContext.Context.Response.Headers.CacheControl = "no-cache, no-store";
        app.UseStaticFiles(sfo);

        static bool UseEmbeddedStaticFiles(Assembly assembly) =>
            assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
                .FirstOrDefault(x => x.Key == "UseEmbeddedStaticFiles")
                ?.Value == "true";
    }

    private static void MapRoutes(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api")
            .RequireAuthorization(
                new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(Constants.DoubleTokenAuthenticationSchemeName)
                    .RequireUserName(Constants.MainUserName)
                    .Build());

        group.MapPost("/auth/login", Route.ExecAnon<AuthLoginParams>).RateLimit().AddMeta();
        group.MapPost("/auth/logout", Route.Exec<AuthLogoutParams>).AddMeta();

        group.MapGet("/files/list", Route.ExecAnon<FilesListParams>).AddMeta();
        group.MapGet("/files/downloadanon/{*filePath}", Route.ExecAnon<FilesDownloadAnonParams>).AddMeta();
        group.MapGet("/files/viewanon/{*filePath}", Route.ExecAnon<FilesViewAnonParams>).AddMeta();
        group.MapGet("/files/download/{*filePath}", Route.Exec<FilesDownloadParams>).AddMeta();
        group.MapGet("/files/view/{*filePath}", Route.Exec<FilesViewParams>).AddMeta();
        group.MapPost("/files/upload", Route.Exec<FilesUploadParams>).AddMeta();
    }

    private static void UseNoCacheHeaders(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            if (context.Request.Method == HttpMethod.Get.Method)
                context.Response.Headers.CacheControl = "no-cache, no-store, private";
            await next();
        });
    }

    private static RouteHandlerBuilder RateLimit(this RouteHandlerBuilder routeHandlerBuilder) =>
        routeHandlerBuilder.RequireRateLimiting(Constants.PerRouteAndIpRateLimitPolicyName);
}
