using System.Security.Claims;
using FileServer.Configuration;
using FileServer.Models.Files;
using FileServer.Services;
using Microsoft.Extensions.Options;
using FileInfo = FileServer.Models.Files.FileInfo;

namespace FileServer.Routes.Files.List;

internal sealed class FilesListHandler(
    IHttpContextAccessor httpContextAccessor,
    IOptionsMonitor<Settings> options,
    FileService fileService)
    : IRouteHandler<FilesListParams>
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IOptionsMonitor<Settings> _options = options;
    private readonly FileService _fileService = fileService;

    private HttpContext Context => _httpContextAccessor.HttpContext!;

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

    private bool IsAuthenticated()
    {
        ClaimsPrincipal user = Context.User;
        return user.Identities.Any(x =>
            x.IsAuthenticated
            && x.AuthenticationType == Constants.DoubleTokenAuthenticationSchemeAuthenticationType
            && x.Name == Constants.MainUserName);
    }
}
