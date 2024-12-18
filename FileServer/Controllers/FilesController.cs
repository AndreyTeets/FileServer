using FileServer.Configuration;
using FileServer.Models;
using FileServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using FileInfo = FileServer.Models.FileInfo;

namespace FileServer.Controllers;

[Route("api/files")]
public class FilesController : ControllerBase
{
    private readonly IOptions<Settings> _options;
    private readonly FileService _fileService;

    public FilesController(IOptions<Settings> options, FileService fileService)
    {
        _options = options;
        // TODO Load settings from env variables with full paths using MSBuildProjectDirectory
        _options.Value.DownloadDir = Path.GetFullPath(_options.Value.DownloadDir!);
        _options.Value.UploadDir = Path.GetFullPath(_options.Value.UploadDir!);
        _fileService = fileService;
    }

    [HttpGet("list")]
    [Produces("application/json")]
    public ActionResult<GetFilesListResponse> GetFilesList()
    {
        List<FileInfo> files = _fileService.GetDownloadableFilesList();
        return new GetFilesListResponse()
        {
            Files = files,
            Count = files.Count,
        };
    }

    [HttpGet($"download/{{*{nameof(filePath)}}}")]
    [Produces("application/octet-stream", "application/json")]
    public ActionResult GetFile([FromRoute] string filePath)
    {
        PhysicalFileProvider fileProvider = new(_options.Value.DownloadDir!);
        IFileInfo file = fileProvider.GetFileInfo(filePath);
        if (!file.Exists)
            return BadRequest("File not found.");
        return PhysicalFile(file.PhysicalPath!, "application/octet-stream");
    }

    [HttpPost("upload")]
    [Produces("application/json")]
    [DisableRequestSizeLimit]
    public async Task<ActionResult<UploadFileResponse>> UploadFile()
    {
        string? formDataBoundary = TryGetFormDataBoundary();
        if (formDataBoundary is null)
            return BadRequest("Not a form-data request.");

        (string? fileName, Stream fileContent) = await TryGetFirstFormDataFile(formDataBoundary!);
        if (fileName is not null)
        {
            (string targetFileName, bool saved) = await _fileService.SaveFileIfNotExists(fileName, fileContent);
            if (saved)
                return new UploadFileResponse() { CreatedFileName = targetFileName };
            else
                return BadRequest($"File with name '{targetFileName}' already exists.");
        }

        return BadRequest("No files in request.");
    }

    private string? TryGetFormDataBoundary()
    {
        if (Request.HasFormContentType
            && MediaTypeHeaderValue.TryParse(Request.ContentType, out MediaTypeHeaderValue? mediaTypeHeader)
            && !string.IsNullOrEmpty(mediaTypeHeader.Boundary.Value))
        {
            return mediaTypeHeader.Boundary.Value;
        }
        return null;
    }

    private async Task<(string? fileName, Stream fileContent)> TryGetFirstFormDataFile(string formDataBoundary)
    {
        MultipartReader reader = new(formDataBoundary, Request.Body);
        MultipartSection? section = await reader.ReadNextSectionAsync();
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

            section = await reader.ReadNextSectionAsync();
        }
        return (null, Stream.Null);
    }
}
