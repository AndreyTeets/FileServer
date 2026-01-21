namespace FileServer.Models.Auth;

internal sealed class LoginInfo
{
    public required string User { get; init; }
    public required DateTime TokensExpire { get; init; }
}
