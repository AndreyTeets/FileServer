namespace FileServer.Models.Files;

internal sealed class FileInfo
{
    public required string Name { get; set; }
    public required string Path { get; set; }
    public required long Size { get; set; }
    public string? Anon { get; set; }
}
