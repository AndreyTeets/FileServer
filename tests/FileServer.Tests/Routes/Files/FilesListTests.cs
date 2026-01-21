using System.Net;
using System.Net.Http.Json;
using FileServer.Models.Files;
using Microsoft.AspNetCore.Http;

namespace FileServer.Tests.Routes.Files;

internal sealed class FilesListTests : ServerTestsBase
{
    [Test]
    public async Task List_NoAuth_ReturnsOnlyAnonFiles()
    {
        using HttpResponseMessage response = await FsTestClient.Get("/api/files/list");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        GetFilesListResponse? files = await response.Content.ReadFromJsonAsync<GetFilesListResponse>();
        Assert.That(files, Is.Not.Null);
        Assert.That(files.Files.Select(x => x.Name), Is.EquivalentTo(["anonfile1.txt"]));
    }

    [Test]
    public async Task List_WithAuth_ReturnsAllFiles()
    {
        await FsTestClient.Login();
        using HttpResponseMessage response = await FsTestClient.Get("/api/files/list");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        GetFilesListResponse? files = await response.Content.ReadFromJsonAsync<GetFilesListResponse>();
        Assert.That(files, Is.Not.Null);
        Assert.That(files.Files.Select(x => x.Name), Is.EquivalentTo(["anonfile1.txt", "file1.txt"]));
    }
}
