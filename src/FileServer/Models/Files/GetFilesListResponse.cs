namespace FileServer.Models.Files;

internal sealed class GetFilesListResponse
{
    public required IEnumerable<FileInfo> Files { get; init; }
    public required int Count { get; init; }
}
