using System.Net;
using System.Text;

namespace FileServer.Tests.Routes.Files;

internal sealed class FilesUploadTests : ServerTestsBase
{
    [Test]
    public async Task Upload_NoAuth_Fails()
    {
        using HttpResponseMessage response = await FsTestClient.Post("/api/files/upload", CreateTestFileContent());
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Upload_WithAuth_Works()
    {
        await FsTestClient.Login();
        using HttpResponseMessage response = await FsTestClient.Post("/api/files/upload", CreateTestFileContent());
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(File.Exists("fs_data/upload/upl.uplfile1.txt.oad"), Is.True);
        Assert.That(await File.ReadAllTextAsync("fs_data/upload/upl.uplfile1.txt.oad"), Is.EqualTo("test_uplfile1_content"));
    }

    [Test]
    public async Task Upload_AlreadyExistingFile_Fails()
    {
        await FsTestClient.Login();
        using HttpResponseMessage _ = await FsTestClient.Post("/api/files/upload", CreateTestFileContent());
        using HttpResponseMessage response = await FsTestClient.Post("/api/files/upload", CreateTestFileContent());
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        Assert.That(await GetContent(response), Is.EqualTo(@"""File with name 'upl.uplfile1.txt.oad' already exists."""));
    }

#pragma warning disable CA2000 // Dispose objects before losing scope
    private static MultipartFormDataContent CreateTestFileContent() => new()
    {
        { new ByteArrayContent(Encoding.UTF8.GetBytes("test_uplfile1_content")), "not_used", "uplfile1.txt" },
    };
#pragma warning restore CA2000 // Responsibility of the types containing it
}
