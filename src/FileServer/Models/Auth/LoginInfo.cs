namespace FileServer.Models.Auth;

internal sealed class LoginInfo
{
    public required string User { get; set; }
    public required DateTime TokensExpire { get; set; }
}
