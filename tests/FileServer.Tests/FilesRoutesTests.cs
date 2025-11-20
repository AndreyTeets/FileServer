using System.Net;
using System.Net.Http.Json;
using System.Text;
using FileServer.Models;
using Microsoft.AspNetCore.Http;

namespace FileServer.Tests;

internal sealed class FilesRoutesTests : TestsBase
{
    [Test]
    public async Task GetFileEndpoints_CorrectlyHandle_RelativeDirs_And_NotFoundFiles()
    {
        HttpContext context = await TestServer.SendAsync(c =>
        {
            c.Request.Method = HttpMethods.Get;
            c.Request.Path = "/api/files/downloadanon/../download/file1.txt";
        });

        Assert.That(context.Response.StatusCode, Is.EqualTo(404));
        Assert.That(GetContent(context), Is.EqualTo(@"""File not found."""));
    }

    [Test]
    public async Task GetFilesList_Without_Auth_ReturnsOnlyAnonFiles()
    {
        using HttpResponseMessage response = await _fsTestClient.Get("/api/files/list");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        GetFilesListResponse? files = await response.Content.ReadFromJsonAsync<GetFilesListResponse>();
        Assert.That(files, Is.Not.Null);
        Assert.That(files.Files!.Select(x => x.Name), Is.EquivalentTo(["anonfile1.txt"]));
    }

    [Test]
    public async Task GetFilesList_With_Auth_ReturnsAllFiles()
    {
        await _fsTestClient.Login();
        using HttpResponseMessage response = await _fsTestClient.Get("/api/files/list");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        GetFilesListResponse? files = await response.Content.ReadFromJsonAsync<GetFilesListResponse>();
        Assert.That(files, Is.Not.Null);
        Assert.That(files.Files!.Select(x => x.Name), Is.EquivalentTo(["anonfile1.txt", "file1.txt"]));
    }

    [Test]
    public async Task DownloadAnonFile_Without_Auth_Works()
    {
        using HttpResponseMessage response = await _fsTestClient.Get("/api/files/downloadanon/anonfile1.txt");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(await GetContent(response), Is.EqualTo("test_anonfile1_content"));
    }

    [Test]
    public async Task ViewAnonFile_Without_Auth_Works()
    {
        using HttpResponseMessage response = await _fsTestClient.Get("/api/files/viewanon/anonfile1.txt");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(await GetContent(response), Is.EqualTo("test_anonfile1_content"));
    }

    [Test]
    public async Task DownloadAnonFile_With_Auth_Works()
    {
        await _fsTestClient.Login();
        using HttpResponseMessage response = await _fsTestClient.Get("/api/files/downloadanon/anonfile1.txt");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(await GetContent(response), Is.EqualTo("test_anonfile1_content"));
    }

    [Test]
    public async Task ViewAnonFile_With_Auth_Works()
    {
        await _fsTestClient.Login();
        using HttpResponseMessage response = await _fsTestClient.Get("/api/files/viewanon/anonfile1.txt");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(await GetContent(response), Is.EqualTo("test_anonfile1_content"));
    }

    [Test]
    public async Task DownloadFile_Without_Auth_IsForbidden()
    {
        using HttpResponseMessage response = await _fsTestClient.Get("/api/files/download/file1.txt");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ViewFile_Without_Auth_IsForbidden()
    {
        using HttpResponseMessage response = await _fsTestClient.Get("/api/files/view/file1.txt");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task DownloadFile_With_Auth_Works()
    {
        await _fsTestClient.Login();
        using HttpResponseMessage response = await _fsTestClient.Get("/api/files/download/file1.txt");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(await GetContent(response), Is.EqualTo("test_file1_content"));
    }

    [Test]
    public async Task ViewFile_With_Auth_Works()
    {
        await _fsTestClient.Login();
        using HttpResponseMessage response = await _fsTestClient.Get("/api/files/view/file1.txt");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(await GetContent(response), Is.EqualTo("test_file1_content"));
    }

    [Test]
    public async Task UploadFile_Without_Auth_IsForbidden()
    {
        using HttpResponseMessage response = await _fsTestClient.Post("/api/files/upload", CreateTestFileContent());
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task UploadFile_With_Auth_Works()
    {
        await _fsTestClient.Login();
        using HttpResponseMessage response = await _fsTestClient.Post("/api/files/upload", CreateTestFileContent());
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(File.Exists("fs_data/upload/upl.uplfile1.txt.oad"), Is.True);
        Assert.That(await File.ReadAllTextAsync("fs_data/upload/upl.uplfile1.txt.oad"), Is.EqualTo("test_uplfile1_content"));
    }

    [Test]
    public async Task UploadFile_AlreadyExistingFile_Fails()
    {
        await _fsTestClient.Login();
        using HttpResponseMessage _ = await _fsTestClient.Post("/api/files/upload", CreateTestFileContent());
        using HttpResponseMessage response = await _fsTestClient.Post("/api/files/upload", CreateTestFileContent());
        Assert.That(await GetContent(response), Is.EqualTo(@"""File with name 'upl.uplfile1.txt.oad' already exists."""));
    }

    private static string GetContent(HttpContext context)
    {
        using StreamReader reader = new(context.Response.Body, Encoding.UTF8);
        return reader.ReadToEnd();
    }

#pragma warning disable CA2000 // Dispose objects before losing scope
    private static MultipartFormDataContent CreateTestFileContent() => new()
    {
        { new ByteArrayContent(Encoding.UTF8.GetBytes("test_uplfile1_content")), "not_used", "uplfile1.txt" },
    };
#pragma warning restore CA2000 // Responsibility of the types containing it
}
