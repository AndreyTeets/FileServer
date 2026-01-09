namespace FileServer.Models;

public class LoginResponse
{
    public required LoginInfo LoginInfo { get; set; }
    public required string AntiforgeryToken { get; set; }
}
