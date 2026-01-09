namespace FileServer.Models.Auth;

internal sealed class LoginRequest
{
    public required string? Password { get; set; }
}
