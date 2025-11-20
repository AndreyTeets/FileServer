namespace FileServer.Models;

public class Token
{
    public Claim? Claim { get; set; }
    public string? Signature { get; set; }
}
