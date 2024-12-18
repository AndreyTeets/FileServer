namespace FileServer.Models;

public class GetFilesListResponse
{
    public IEnumerable<FileInfo>? Files { get; set; }
    public int Count { get; set; }
}
