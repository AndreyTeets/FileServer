using System.Text;
using FileServer.Tests.Util;
using Microsoft.AspNetCore.TestHost;

namespace FileServer.Tests;

public abstract class TestsBase : ILoggedTest
{
    private protected HttpClient _testClient;
    protected static TestServer TestServer => SetupTestServerFixture.Host.GetTestServer();
    private protected FileServerTestClient _fsTestClient;

    public StringBuilder LogsSb => SetupTestServerFixture.LogsSb;

    [SetUp]
    public void SetUpTestClients()
    {
        foreach (FileInfo file in new DirectoryInfo(Path.GetFullPath("fs_data/upload")).GetFiles())
            file.Delete();

        _testClient = SetupTestServerFixture.Host.GetTestClient();
        CookieProcessingHttpMessageHandler cpHttpClientHandler = new(TestServer.CreateHandler());
        HttpClient cpHttpClient = new(cpHttpClientHandler);
        cpHttpClient.BaseAddress = _testClient.BaseAddress;
        _fsTestClient = new FileServerTestClient(cpHttpClient, cpHttpClientHandler);
    }

    [TearDown]
    public void TearDownTestClients()
    {
        _fsTestClient?.Dispose();
        _testClient?.Dispose();
    }

    protected static async Task<string> GetContent(HttpResponseMessage response)
    {
        return await response.Content.ReadAsStringAsync();
    }
}
