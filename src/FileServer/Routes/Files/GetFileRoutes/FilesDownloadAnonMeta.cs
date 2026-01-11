using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;

namespace FileServer.Routes.Files.GetFileRoutes;

[ProducesResponseType(typeof(void), StatusCodes.Status200OK, MediaTypeNames.Application.Octet)]
[ProducesResponseType(typeof(string), StatusCodes.Status404NotFound, MediaTypeNames.Application.Json)]
[Tags(Constants.FilesRouteTag)]
internal sealed class FilesDownloadAnonMeta : IRouteMeta<FilesDownloadAnonParams>;
