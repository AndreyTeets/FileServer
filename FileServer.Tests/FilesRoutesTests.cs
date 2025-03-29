using System.Net;
using System.Text;

namespace FileServer.Tests;

public class FilesRoutesTests : TestsBase
{
    [Test]
    public async Task GetFilesList_Without_Auth_IsForbidden()
    {
        using HttpResponseMessage response = await FsTestClient.Get("/api/files/list");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task DownloadFile_Without_Auth_IsForbidden()
    {
        using HttpResponseMessage response = await FsTestClient.Get("/api/files/download/file1.txt");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task DownloadFile_With_Auth_Works()
    {
        await FsTestClient.Login();
        using HttpResponseMessage response = await FsTestClient.Get("/api/files/download/file1.txt");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(await GetContent(response), Is.EqualTo("test_file1_content"));
    }

    [Test]
    public async Task UploadFile_Without_Auth_IsForbidden()
    {
        using HttpResponseMessage response = await FsTestClient.Post("/api/files/upload", CreateTestFileContent());
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task UploadFile_With_Auth_Works()
    {
        await FsTestClient.Login();
        using HttpResponseMessage response = await FsTestClient.Post("/api/files/upload", CreateTestFileContent());
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(File.Exists("fs_data/upload/upl.uplfile1.txt.oad"), Is.True);
        Assert.That(File.ReadAllText("fs_data/upload/upl.uplfile1.txt.oad"), Is.EqualTo("test_uplfile1_content"));
    }

    [Test]
    public async Task UploadFile_AlreadyExistingFile_Fails()
    {
        await FsTestClient.Login();
        using HttpResponseMessage _ = await FsTestClient.Post("/api/files/upload", CreateTestFileContent());
        using HttpResponseMessage response = await FsTestClient.Post("/api/files/upload", CreateTestFileContent());
        Assert.That(await GetContent(response), Is.EqualTo(@"""File with name 'upl.uplfile1.txt.oad' already exists."""));
    }

    private static MultipartFormDataContent CreateTestFileContent()
    {
        return new()
        {
            { new ByteArrayContent(Encoding.UTF8.GetBytes("test_uplfile1_content")), "not_used", "uplfile1.txt" }
        };
    }
}
