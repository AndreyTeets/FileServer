using System.Net.Mime;
using FileServer.Models.Auth;
using Microsoft.AspNetCore.Mvc;

namespace FileServer.Routes.Auth.Login;

[ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[Tags(Constants.AuthRouteTag)]
internal sealed class AuthLoginMeta : IRouteMeta<AuthLoginParams>;
