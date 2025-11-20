namespace FileServer.Models;

public class FileInfo
{
    public string? Name { get; set; }
    public string? Path { get; set; }
    public long Size { get; set; }
    public string? Anon { get; set; }
}
