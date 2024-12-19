namespace FileServer.Models;

public class Claim
{
    public string? User { get; set; }
    public string? Type { get; set; }
    public DateTime? Expires { get; set; }
}
