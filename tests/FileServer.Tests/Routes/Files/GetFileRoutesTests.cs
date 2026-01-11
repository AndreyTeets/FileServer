using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace FileServer.Tests.Routes.Files;

internal sealed class GetFileRoutesTests : TestsBase
{
    [Test]
    public async Task GetFileRoutes_CorrectlyHandle_RelativeDirs_And_NotFoundFiles()
    {
        HttpContext context = await TestServer.SendAsync(c =>
        {
            c.Request.Method = HttpMethods.Get;
            c.Request.Path = "/api/files/downloadanon/../download/file1.txt";
        });

        Assert.That(context.Response.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
        Assert.That(GetContent(context), Is.EqualTo(@"""File not found."""));
    }

    [Test]
    public async Task Download_NoAuth_Fails()
    {
        using HttpResponseMessage response = await _fsTestClient.Get("/api/files/download/file1.txt");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Download_WithAuth_Works()
    {
        await _fsTestClient.Login();
        using HttpResponseMessage response = await _fsTestClient.Get("/api/files/download/file1.txt");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(await GetContent(response), Is.EqualTo("test_file1_content"));
    }

    [Test]
    public async Task DownloadAnon_NoAuth_Works()
    {
        using HttpResponseMessage response = await _fsTestClient.Get("/api/files/downloadanon/anonfile1.txt");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(await GetContent(response), Is.EqualTo("test_anonfile1_content"));
    }

    [Test]
    public async Task DownloadAnon_WithAuth_Works()
    {
        await _fsTestClient.Login();
        using HttpResponseMessage response = await _fsTestClient.Get("/api/files/downloadanon/anonfile1.txt");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(await GetContent(response), Is.EqualTo("test_anonfile1_content"));
    }

    [Test]
    public async Task View_NoAuth_Fails()
    {
        using HttpResponseMessage response = await _fsTestClient.Get("/api/files/view/file1.txt");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task View_WithAuth_Works()
    {
        await _fsTestClient.Login();
        using HttpResponseMessage response = await _fsTestClient.Get("/api/files/view/file1.txt");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(await GetContent(response), Is.EqualTo("test_file1_content"));
    }

    [Test]
    public async Task ViewAnon_NoAuth_Works()
    {
        using HttpResponseMessage response = await _fsTestClient.Get("/api/files/viewanon/anonfile1.txt");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(await GetContent(response), Is.EqualTo("test_anonfile1_content"));
    }

    [Test]
    public async Task ViewAnon_WithAuth_Works()
    {
        await _fsTestClient.Login();
        using HttpResponseMessage response = await _fsTestClient.Get("/api/files/viewanon/anonfile1.txt");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(await GetContent(response), Is.EqualTo("test_anonfile1_content"));
    }

    private static string GetContent(HttpContext context)
    {
        using StreamReader reader = new(context.Response.Body, Encoding.UTF8);
        return reader.ReadToEnd();
    }
}
