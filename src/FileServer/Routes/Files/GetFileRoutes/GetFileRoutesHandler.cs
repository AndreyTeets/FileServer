using System.Net.Mime;
using FileServer.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace FileServer.Routes.Files.GetFileRoutes;

internal sealed class GetFileRoutesHandler(
    IOptionsMonitor<Settings> options)
    : IRouteHandler<FilesDownloadAnonParams>
    , IRouteHandler<FilesViewAnonParams>
    , IRouteHandler<FilesDownloadParams>
    , IRouteHandler<FilesViewParams>
{
    private readonly IOptionsMonitor<Settings> _options = options;

    public async Task<IResult> Execute(FilesDownloadAnonParams routeParams) =>
        CreateGetFileResult(_options.CurrentValue.DownloadAnonDir, MediaTypeNames.Application.Octet, routeParams.FilePath);

    public async Task<IResult> Execute(FilesViewAnonParams routeParams) =>
        CreateGetFileResult(_options.CurrentValue.DownloadAnonDir, MediaTypeNames.Text.Plain, routeParams.FilePath);

    public async Task<IResult> Execute(FilesDownloadParams routeParams) =>
        CreateGetFileResult(_options.CurrentValue.DownloadDir, MediaTypeNames.Application.Octet, routeParams.FilePath);

    public async Task<IResult> Execute(FilesViewParams routeParams) =>
        CreateGetFileResult(_options.CurrentValue.DownloadDir, MediaTypeNames.Text.Plain, routeParams.FilePath);

    private static IResult CreateGetFileResult(string rootDir, string mimeType, string filePath)
    {
        using PhysicalFileProvider fileProvider = new(rootDir);
        IFileInfo file = fileProvider.GetFileInfo(filePath);
        return file.Exists
            ? Results.File(file.PhysicalPath!, mimeType)
            : Results.NotFound("File not found.");
    }
}
