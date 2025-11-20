namespace FileServer.Models;

public class LoginInfo
{
    public string? User { get; set; }
    public DateTime? TokensExpire { get; set; }
}
