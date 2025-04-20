using System.Text;
using FileServer.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using FileInfo = FileServer.Models.FileInfo;

namespace FileServer.Services;

public class FileService(
    IOptionsMonitor<Settings> options)
{
    private readonly IOptionsMonitor<Settings> _options = options;

    public List<FileInfo> GetDownloadableFilesList(string rootDir)
    {
        using PhysicalFileProvider fileProvider = new(rootDir);
        List<FileInfo> files = [];
        FillFilesListRecursive(rootDir, files, fileProvider, "");
        return files;
    }

    public async Task<(string targetFileName, bool saved)> SaveFileIfNotExists(
        string originalFileName, Stream fileContent)
    {
        string trustedFileName = SanitizeFileName(originalFileName);
        string saveToPath = Path.Combine(_options.CurrentValue.UploadDir!, trustedFileName);

        if (!File.Exists(saveToPath))
        {
            await using FileStream targetStream = File.Create(saveToPath);
            await fileContent.CopyToAsync(targetStream);
            return (trustedFileName, true);
        }
        return (trustedFileName, false);
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
            FileInfo fileInfo = new()
            {
                Name = file.Name,
                Path = relativeFilePath.Replace("\\", "/"),
                Size = file.Length,
            };
            files.Add(fileInfo);
        }
    }

    private static string SanitizeFileName(string fileName)
    {
        string cleanedFileName = ReplaceNonWhitelistedChars(fileName).Trim();
        string shortenedFileName = cleanedFileName[..Math.Min(cleanedFileName.Length, 120)];
        return "upl." + shortenedFileName + ".oad";
    }

    private static string ReplaceNonWhitelistedChars(string fileName)
    {
        StringBuilder sb = new();
        foreach (char c in fileName)
        {
            if (c == 32 || // Space
                c == 33 || // !
                c == 35 || // #
                c == 36 || // $
                c == 37 || // %
                c == 38 || // &
                c == 39 || // '
                c == 40 || // (
                c == 41 || // )
                c == 43 || // +
                c == 44 || // ,
                c == 45 || // -
                c == 46 || // .
                c >= 48 && c <= 57 || // 0-9
                c == 59 || // ;
                c == 61 || // =
                c == 64 || // @
                c >= 65 && c <= 90 || // A-Z
                c == 91 || // [
                c == 93 || // ]
                c == 94 || // ^
                c == 95 || // _
                c == 96 || // `
                c >= 97 && c <= 122 || // a-z
                c == 123 || // {
                c == 125 || // }
                c == 126) // ~
            {
                sb.Append(c);
            }
            else
            {
                sb.Append('_');
            }
        }
        return sb.ToString();
    }
}
