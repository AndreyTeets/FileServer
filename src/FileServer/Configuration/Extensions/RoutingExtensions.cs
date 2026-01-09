using System.Reflection;
using FileServer.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.FileProviders;

namespace FileServer.Configuration.Extensions;

internal static class RoutingExtensions
{
    public static void UseToIndexPageRedirect(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            if (context.Request.Path.Value == "/")
                context.Request.Path = "/index.html";
            await next();
        });
    }

    public static void UseStaticFilesWithNoCacheHeaders(this IApplicationBuilder app)
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

    public static void MapRoutes(this IEndpointRouteBuilder endpoints, IServiceProvider services)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api")
            .RequireAuthorization(
                new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(Constants.DoubleTokenAuthenticationSchemeName)
                    .RequireUserName(Constants.MainUserName)
                    .Build());

        AuthController authController = services.GetRequiredService<AuthController>();
        group.MapPost("/auth/login", authController.Login);
        group.MapPost("/auth/logout", authController.Logout);

        FilesController filesController = services.GetRequiredService<FilesController>();
        group.MapGet("/files/list", filesController.GetFilesList);
        group.MapGet("/files/downloadanon/{*filePath}", filesController.DownloadFileAnon);
        group.MapGet("/files/viewanon/{*filePath}", filesController.ViewFileAnon);
        group.MapGet("/files/download/{*filePath}", filesController.DownloadFile);
        group.MapGet("/files/view/{*filePath}", filesController.ViewFile);
        group.MapPost("/files/upload", filesController.UploadFile);
    }

    public static void UseNoCacheHeaders(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            if (context.Request.Method == HttpMethod.Get.Method)
                context.Response.Headers.CacheControl = "no-cache, no-store, private";
            await next();
        });
    }
}
