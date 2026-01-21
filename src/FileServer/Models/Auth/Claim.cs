namespace FileServer.Models.Auth;

internal sealed class Claim
{
    public required string User { get; init; }
    public required string Type { get; init; }
    public required DateTime Expires { get; init; }
}
