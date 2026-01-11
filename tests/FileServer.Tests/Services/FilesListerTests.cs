using FileServer.Services;
using Microsoft.Extensions.DependencyInjection;
using FileInfo = FileServer.Models.Files.FileInfo;

namespace FileServer.Tests.Services;

internal sealed class FilesListerTests : TestsBase
{
#pragma warning disable CS8618 // Non-nullable variable must contain a non-null value when exiting constructor. Consider declaring it as nullable.
    private FilesLister _filesLister;
#pragma warning restore CS8618 // Remove when `dotnet format` is fixed (see https://github.com/dotnet/sdk/issues/44867)

    [SetUp]
    public void SetUp()
    {
        _filesLister = TestServer.Services.GetRequiredService<FilesLister>();
    }

    [Test]
    public async Task GetDirectoryFilesListRecursive_FindsAllFiles()
    {
        List<FileInfo> x = _filesLister.GetDirectoryFilesListRecursive(Path.GetFullPath("fs_data"));
        Assert.That(x.Select(x => x.Path), Is.EquivalentTo(
        [   // All files in upload folder are deleted in the tests base setup
            "download/file1.txt",
            "download_anon/anonfile1.txt",
        ]));
    }
}
