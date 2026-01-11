using FileServer.Models.Files;
using FileServer.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace FileServer.Routes.Files.Upload;

internal sealed class FilesUploadHandler(
    IHttpContextAccessor httpContextAccessor,
    FileSaver fileSaver)
    : IRouteHandler<FilesUploadParams>
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly FileSaver _fileSaver = fileSaver;

    private HttpContext Context => _httpContextAccessor.HttpContext!;
    private HttpRequest Request => _httpContextAccessor.HttpContext!.Request;
    private CancellationToken Ct => _httpContextAccessor.HttpContext!.RequestAborted;

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

        (string targetFileName, bool saved) = await _fileSaver.SaveFileIfNotExists(fileName, fileContent, Ct);
        return saved
            ? Results.Ok(new UploadFileResponse() { CreatedFileName = targetFileName })
            : Results.Conflict($"File with name '{targetFileName}' already exists.");
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
}
