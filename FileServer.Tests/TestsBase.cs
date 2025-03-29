using System.Text;
using FileServer.Tests.Util;
using Microsoft.AspNetCore.TestHost;

namespace FileServer.Tests;

public abstract class TestsBase : ILoggedTest
{
    protected HttpClient TestClient;
    protected TestServer TestServer => SetupTestServerFixture.Host.GetTestServer();
    protected FileServerTestClient FsTestClient;

    public StringBuilder LogsSb => SetupTestServerFixture.LogsSb;

    [SetUp]
    public void SetUpTestClients()
    {
        foreach (FileInfo file in new DirectoryInfo(Path.GetFullPath("fs_data/upload")).GetFiles())
            file.Delete();

        TestClient = SetupTestServerFixture.Host.GetTestClient();
        CookieProcessingHttpMessageHandler cpHttpClientHandler = new(TestServer.CreateHandler());
        HttpClient cpHttpClient = new(cpHttpClientHandler);
        cpHttpClient.BaseAddress = TestClient.BaseAddress;
        FsTestClient = new FileServerTestClient(cpHttpClient, cpHttpClientHandler);
    }

    [TearDown]
    public void TearDownTestClients()
    {
        FsTestClient?.Dispose();
        TestClient?.Dispose();
    }

    protected static async Task<string> GetContent(HttpResponseMessage response)
    {
        return await response.Content.ReadAsStringAsync();
    }
}
