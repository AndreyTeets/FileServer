namespace FileServer.Models.Auth;

internal sealed class LoginResponse
{
    public required LoginInfo LoginInfo { get; init; }
    public required string AntiforgeryToken { get; init; }
}
