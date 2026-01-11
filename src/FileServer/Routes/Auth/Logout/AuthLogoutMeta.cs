using Microsoft.AspNetCore.Mvc;

namespace FileServer.Routes.Auth.Logout;

[ProducesResponseType(StatusCodes.Status200OK)]
[Tags(Constants.AuthRouteTag)]
internal sealed class AuthLogoutMeta : IRouteMeta<AuthLogoutParams>;
