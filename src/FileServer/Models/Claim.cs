namespace FileServer.Models;

public class Claim
{
    public required string User { get; set; }
    public required string Type { get; set; }
    public required DateTime Expires { get; set; }
}
