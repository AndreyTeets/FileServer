namespace FileServer.Models;

public class LoginResponse
{
    public LoginInfo? LoginInfo { get; set; }
    public string? AntiforgeryToken { get; set; }
}
