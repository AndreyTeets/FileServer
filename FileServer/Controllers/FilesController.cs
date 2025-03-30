using System.Security.Claims;
using FileServer.Configuration;
using FileServer.Models;
using FileServer.Services;
using Microsoft.AspNetCore.Authorization;
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
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOptionsMonitor<Settings> _options;
    private readonly FileService _fileService;

    public FilesController(
        IHttpContextAccessor httpContextAccessor,
        IOptionsMonitor<Settings> options,
        FileService fileService)
    {
        _httpContextAccessor = httpContextAccessor;
        _options = options;
        _fileService = fileService;
    }

    [HttpGet("list")]
    [Produces("application/json")]
    [AllowAnonymous]
    public ActionResult<GetFilesListResponse> GetFilesList()
    {
        List<FileInfo> files = _fileService.GetDownloadableFilesList(_options.CurrentValue.DownloadAnonDir!);
        files.ForEach(x => x.Anon = "yes");

        if (IsAuthenticated())
        {
            List<FileInfo> authFiles = _fileService.GetDownloadableFilesList(_options.CurrentValue.DownloadDir!);
            authFiles.ForEach(x => x.Anon = "no");
            files.AddRange(authFiles);
        }

        return new GetFilesListResponse()
        {
            Files = files,
            Count = files.Count,
        };
    }

    [HttpGet($"downloadanon/{{*{nameof(filePath)}}}")]
    [Produces("application/octet-stream", "application/json")]
    [AllowAnonymous]
    public ActionResult DownloadFileAnon([FromRoute] string filePath)
    {
        return CreateGetFileResult(_options.CurrentValue.DownloadAnonDir!, "application/octet-stream", filePath);
    }

    [HttpGet($"viewanon/{{*{nameof(filePath)}}}")]
    [Produces("text/plain", "application/json")]
    [AllowAnonymous]
    public ActionResult ViewFileAnon([FromRoute] string filePath)
    {
        return CreateGetFileResult(_options.CurrentValue.DownloadAnonDir!, "text/plain", filePath);
    }

    [HttpGet($"download/{{*{nameof(filePath)}}}")]
    [Produces("application/octet-stream", "application/json")]
    public ActionResult DownloadFile([FromRoute] string filePath)
    {
        return CreateGetFileResult(_options.CurrentValue.DownloadDir!, "application/octet-stream", filePath);
    }

    [HttpGet($"view/{{*{nameof(filePath)}}}")]
    [Produces("text/plain", "application/json")]
    public ActionResult ViewFile([FromRoute] string filePath)
    {
        return CreateGetFileResult(_options.CurrentValue.DownloadDir!, "text/plain", filePath);
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

    private bool IsAuthenticated()
    {
        ClaimsPrincipal user = _httpContextAccessor.HttpContext!.User;
        bool isAuthenticated = user.Identities.Any(x =>
            x.IsAuthenticated == true
            && x.AuthenticationType == Constants.DoubleTokenAuthenticationSchemeAuthenticationType
            && x.Name == Constants.MainUserName);
        return isAuthenticated;
    }

    private ActionResult CreateGetFileResult(string rootDir, string mimeType, string filePath)
    {
        PhysicalFileProvider fileProvider = new(rootDir);
        IFileInfo file = fileProvider.GetFileInfo(filePath);
        if (!file.Exists)
            return BadRequest("File not found.");
        return PhysicalFile(file.PhysicalPath!, mimeType);
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
