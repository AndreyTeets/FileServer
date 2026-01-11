using Microsoft.AspNetCore.Mvc;

namespace FileServer.Routes.Files.GetFileRoutes;

internal sealed class FilesDownloadAnonParams
{
    [FromRoute]
    public required string FilePath { get; set; }
}
