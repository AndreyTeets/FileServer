using FileServer.Services;
using Microsoft.Extensions.DependencyInjection;
using FileInfo = FileServer.Models.Files.FileInfo;

namespace FileServer.Tests.Services;

internal sealed class FilesListerTests : ServerTestsBase
{
    private FilesLister _filesLister;

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
