using Microsoft.Extensions.FileProviders;
using FileInfo = FileServer.Models.Files.FileInfo;

namespace FileServer.Services;

internal sealed class FilesLister
{
    public List<FileInfo> GetDirectoryFilesListRecursive(string rootDir)
    {
        using PhysicalFileProvider fileProvider = new(rootDir);
        List<FileInfo> files = [];
        FillFilesListRecursive(rootDir, files, fileProvider, "");
        return files;
    }

    private static void FillFilesListRecursive(
        string rootDir, List<FileInfo> files, PhysicalFileProvider fileProvider, string subPath)
    {
        IEnumerable<IFileInfo> dirItems = fileProvider.GetDirectoryContents(subPath);
        foreach (IFileInfo dir in dirItems.Where(f => f.IsDirectory))
        {
            string relativeDirPath = Path.GetRelativePath(rootDir, dir.PhysicalPath!);
            FillFilesListRecursive(rootDir, files, fileProvider, relativeDirPath);
        }
        foreach (IFileInfo file in dirItems.Where(f => !f.IsDirectory))
        {
            string relativeFilePath = Path.GetRelativePath(rootDir, file.PhysicalPath!);
            files.Add(new FileInfo()
            {
                Name = file.Name,
                Path = relativeFilePath.Replace("\\", "/"),
                Size = file.Length,
            });
        }
    }
}
