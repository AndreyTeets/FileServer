namespace FileServer.Models;

internal sealed class Claim
{
    public required string User { get; set; }
    public required string Type { get; set; }
    public required DateTime Expires { get; set; }
}
