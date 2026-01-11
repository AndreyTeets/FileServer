using FileServer.Models.Auth;
using Microsoft.AspNetCore.Mvc;

namespace FileServer.Routes.Auth.Login;

internal sealed class AuthLoginParams
{
    [FromBody]
    public required LoginRequest Request { get; set; }
}
