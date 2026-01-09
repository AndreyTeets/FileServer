using System.Text;
using FileServer.Services;
using Microsoft.Extensions.DependencyInjection;
using FileInfo = FileServer.Models.Files.FileInfo;

namespace FileServer.Tests.Services;

internal sealed class FileServiceTests : TestsBase
{
#pragma warning disable CS8618 // Non-nullable variable must contain a non-null value when exiting constructor. Consider declaring it as nullable.
    private FileService _fileService;
#pragma warning restore CS8618 // Remove when `dotnet format` is fixed (see https://github.com/dotnet/sdk/issues/44867)

    [SetUp]
    public void SetUp()
    {
        _fileService = TestServer.Services.GetRequiredService<FileService>();
    }

    [Test]
    public async Task GetDirectoryFilesListRecursive_FindsAllFiles()
    {
        List<FileInfo> x = _fileService.GetDirectoryFilesListRecursive(Path.GetFullPath("fs_data"));
        Assert.That(x.Select(x => x.Path), Is.EquivalentTo(
        [   // All files in upload folder are deleted in the tests base setup
            "download/file1.txt",
            "download_anon/anonfile1.txt",
        ]));
    }

    [TestCase("", "upl..oad")]
    [TestCase("09AZaz !#$%&'()+,-.;=@[]^_`{}~", "upl.09AZaz !#$%&'()+,-.;=@[]^_`{}~.oad")]
    [TestCase("../X", "upl..._X.oad")]
    [TestCase("1 π 2 π€Й 3 🥰 4 \u222B 5 \ud83e\udd70 0", "upl.1 _ 2 ___ 3 __ 4 _ 5 __ 0.oad")]
    public async Task SaveFileIfNotExists_SavesFileWithSanitizedName(string fileName, string expectedFileName)
    {
        string fileContentText = $"{fileName}\r?\n{expectedFileName}\n";
        using CancellationTokenSource cts = new();
        await using Stream fileContent = CreateFileContent(fileContentText);

        (string targetFileName, bool saved) = await _fileService.SaveFileIfNotExists(fileName, fileContent, cts.Token);
        Assert.That(saved, Is.True);
        Assert.That(targetFileName, Is.EqualTo(expectedFileName));

        string targetFilePath = $"fs_data/upload/{targetFileName}";
        Assert.That(File.Exists(targetFilePath), Is.True);
        Assert.That(await File.ReadAllTextAsync(targetFilePath), Is.EqualTo(fileContentText));
    }

    [Test]
    public async Task SaveFileIfNotExists_SkipsSavingFileWhenExists()
    {
        const string fileName = "does_not_matter";
        const string fileContentText = "does_not_matter";
        using CancellationTokenSource cts = new();
        await using Stream fileContent = CreateFileContent(fileContentText);

        (string _, bool saved1) = await _fileService.SaveFileIfNotExists(fileName, fileContent, cts.Token);
        Assert.That(saved1, Is.True);
        (string _, bool saved2) = await _fileService.SaveFileIfNotExists(fileName, fileContent, cts.Token);
        Assert.That(saved2, Is.False);
    }

    private static Stream CreateFileContent(string text) =>
        new MemoryStream(Encoding.UTF8.GetBytes(text));
}
