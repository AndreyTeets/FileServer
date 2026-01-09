namespace FileServer.Models;

internal sealed class Token
{
    public required Claim Claim { get; set; }
    public required string Signature { get; set; }
}
