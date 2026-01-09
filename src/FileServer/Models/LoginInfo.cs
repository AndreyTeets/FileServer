namespace FileServer.Models;

public class LoginInfo
{
    public required string User { get; set; }
    public required DateTime TokensExpire { get; set; }
}
