using Microsoft.AspNetCore.Mvc;

namespace FileServer.Routes.Files.GetFileRoutes;

internal sealed class FilesDownloadParams
{
    [FromRoute]
    public required string FilePath { get; set; }
}
