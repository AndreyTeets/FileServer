using System.Net.Mime;
using FileServer.Models.Files;
using Microsoft.AspNetCore.Mvc;

namespace FileServer.Routes.Files.List;

[ProducesResponseType(typeof(GetFilesListResponse), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
[Tags(Constants.FilesRouteTag)]
internal sealed class FilesListMeta : IRouteMeta<FilesListParams>;
