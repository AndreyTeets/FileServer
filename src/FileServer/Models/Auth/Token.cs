namespace FileServer.Models.Auth;

internal sealed class Token
{
    public required Claim Claim { get; init; }
    public required string Signature { get; init; }
}
