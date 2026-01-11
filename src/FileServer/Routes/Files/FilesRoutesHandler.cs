using System.Net.Mime;
using System.Security.Claims;
using FileServer.Configuration;
using FileServer.Models.Files;
using FileServer.Routes.Files.GetFileRoutes;
using FileServer.Routes.Files.List;
using FileServer.Routes.Files.Upload;
using FileServer.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using FileInfo = FileServer.Models.Files.FileInfo;

namespace FileServer.Routes.Files;

internal sealed class FilesRoutesHandler(
    IHttpContextAccessor httpContextAccessor,
    IOptionsMonitor<Settings> options,
    FileService fileService)
    : IRouteHandler<FilesListParams>
    , IRouteHandler<FilesDownloadAnonParams>
    , IRouteHandler<FilesViewAnonParams>
    , IRouteHandler<FilesDownloadParams>
    , IRouteHandler<FilesViewParams>
    , IRouteHandler<FilesUploadParams>
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IOptionsMonitor<Settings> _options = options;
    private readonly FileService _fileService = fileService;

    private HttpContext Context => _httpContextAccessor.HttpContext!;
    private HttpRequest Request => _httpContextAccessor.HttpContext!.Request;
    private CancellationToken Ct => _httpContextAccessor.HttpContext!.RequestAborted;

    public async Task<IResult> Execute(FilesListParams routeParams)
    {
        List<FileInfo> files = _fileService.GetDirectoryFilesListRecursive(_options.CurrentValue.DownloadAnonDir);
        files.ForEach(x => x.Anon = "yes");

        if (IsAuthenticated())
        {
            List<FileInfo> authFiles = _fileService.GetDirectoryFilesListRecursive(_options.CurrentValue.DownloadDir);
            authFiles.ForEach(x => x.Anon = "no");
            files.AddRange(authFiles);
        }

        return Results.Ok(new GetFilesListResponse()
        {
            Files = files,
            Count = files.Count,
        });
    }

    public async Task<IResult> Execute(FilesDownloadAnonParams routeParams) =>
        CreateGetFileResult(_options.CurrentValue.DownloadAnonDir, MediaTypeNames.Application.Octet, routeParams.FilePath);

    public async Task<IResult> Execute(FilesViewAnonParams routeParams) =>
        CreateGetFileResult(_options.CurrentValue.DownloadAnonDir, MediaTypeNames.Text.Plain, routeParams.FilePath);

    public async Task<IResult> Execute(FilesDownloadParams routeParams) =>
        CreateGetFileResult(_options.CurrentValue.DownloadDir, MediaTypeNames.Application.Octet, routeParams.FilePath);

    public async Task<IResult> Execute(FilesViewParams routeParams) =>
        CreateGetFileResult(_options.CurrentValue.DownloadDir, MediaTypeNames.Text.Plain, routeParams.FilePath);

    public async Task<IResult> Execute(FilesUploadParams routeParams)
    {
        IHttpMaxRequestBodySizeFeature bodySizeFeature = Context.Features.GetRequiredFeature<IHttpMaxRequestBodySizeFeature>();
        bodySizeFeature.MaxRequestBodySize = null;

        string? formDataBoundary = TryGetFormDataBoundary();
        if (formDataBoundary is null)
            return Results.BadRequest("Not a form-data request.");

        (string? fileName, Stream fileContent) = await TryGetFirstFormDataFile(formDataBoundary);
        if (fileName is null)
            return Results.BadRequest("No files in request.");

        (string targetFileName, bool saved) = await _fileService.SaveFileIfNotExists(fileName, fileContent, Ct);
        return saved
            ? Results.Ok(new UploadFileResponse() { CreatedFileName = targetFileName })
            : Results.Conflict($"File with name '{targetFileName}' already exists.");
    }

    private static IResult CreateGetFileResult(string rootDir, string mimeType, string filePath)
    {
        using PhysicalFileProvider fileProvider = new(rootDir);
        IFileInfo file = fileProvider.GetFileInfo(filePath);
        return file.Exists
            ? Results.File(file.PhysicalPath!, mimeType)
            : Results.NotFound("File not found.");
    }

    private string? TryGetFormDataBoundary()
    {
        return Request.HasFormContentType
                && MediaTypeHeaderValue.TryParse(Request.ContentType, out MediaTypeHeaderValue? mediaTypeHeader)
                && !string.IsNullOrEmpty(mediaTypeHeader.Boundary.Value)
            ? mediaTypeHeader.Boundary.Value
            : null;
    }

    private async Task<(string? fileName, Stream fileContent)> TryGetFirstFormDataFile(string formDataBoundary)
    {
        MultipartReader reader = new(formDataBoundary, Request.Body);
        MultipartSection? section = await reader.ReadNextSectionAsync(Ct);
        while (section != null)
        {
            bool hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(
                section.ContentDisposition,
                out ContentDispositionHeaderValue? contentDisposition);

            if (hasContentDispositionHeader
                && contentDisposition!.DispositionType.Equals("form-data")
                && !string.IsNullOrEmpty(contentDisposition.FileName.Value))
            {
                return (contentDisposition.FileName.Value, section.Body);
            }

            section = await reader.ReadNextSectionAsync(Ct);
        }
        return (null, Stream.Null);
    }

    private bool IsAuthenticated()
    {
        ClaimsPrincipal user = Context.User;
        return user.Identities.Any(x =>
            x.IsAuthenticated
            && x.AuthenticationType == Constants.DoubleTokenAuthenticationSchemeAuthenticationType
            && x.Name == Constants.MainUserName);
    }
}
