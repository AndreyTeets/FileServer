using System.Net.Mime;
using FileServer.Models.Files;
using Microsoft.AspNetCore.Mvc;

namespace FileServer.Routes.Files.Upload;

[ProducesResponseType(typeof(UploadFileResponse), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
[ProducesResponseType(typeof(string), StatusCodes.Status409Conflict, MediaTypeNames.Application.Json)]
[Tags(Constants.FilesRouteTag)]
internal sealed class FilesUploadMeta : IRouteMeta<FilesUploadParams>;
