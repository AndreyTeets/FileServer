namespace FileServer.Models.Files;

internal sealed class GetFilesListResponse
{
    public required IEnumerable<FileInfo> Files { get; set; }
    public required int Count { get; set; }
}
