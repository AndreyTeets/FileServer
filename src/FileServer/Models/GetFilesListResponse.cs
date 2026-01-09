namespace FileServer.Models;

public class GetFilesListResponse
{
    public required IEnumerable<FileInfo> Files { get; set; }
    public required int Count { get; set; }
}
