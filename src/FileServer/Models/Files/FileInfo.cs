namespace FileServer.Models.Files;

internal sealed class FileInfo
{
    public required string Name { get; init; }
    public required string Path { get; init; }
    public required long Size { get; init; }
    public string? Anon { get; set; }
}
