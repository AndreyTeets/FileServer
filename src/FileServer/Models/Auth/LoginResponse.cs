namespace FileServer.Models.Auth;

internal sealed class LoginResponse
{
    public required LoginInfo LoginInfo { get; set; }
    public required string AntiforgeryToken { get; set; }
}
