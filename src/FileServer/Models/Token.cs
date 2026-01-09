namespace FileServer.Models;

public class Token
{
    public required Claim Claim { get; set; }
    public required string Signature { get; set; }
}
