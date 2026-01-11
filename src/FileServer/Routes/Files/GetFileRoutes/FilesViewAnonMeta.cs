using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;

namespace FileServer.Routes.Files.GetFileRoutes;

[ProducesResponseType(typeof(void), StatusCodes.Status200OK, MediaTypeNames.Text.Plain)]
[ProducesResponseType(typeof(string), StatusCodes.Status404NotFound, MediaTypeNames.Application.Json)]
[Tags(Constants.FilesRouteTag)]
internal sealed class FilesViewAnonMeta : IRouteMeta<FilesViewAnonParams>;
