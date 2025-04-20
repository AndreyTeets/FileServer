using System.Text;
using FileServer.Tests.Util;
using Microsoft.AspNetCore.TestHost;

namespace FileServer.Tests;

public abstract class TestsBase : ILoggedTest
{
#pragma warning disable CS8618 // Non-nullable variable must contain a non-null value when exiting constructor. Consider declaring it as nullable.
    private protected HttpClient _testClient;
    private protected FileServerTestClient _fsTestClient;
#pragma warning restore CS8618 // NUnit setup will ensure it's initialized by the time it's used by tests
    protected static TestServer TestServer => SetupTestServerFixture.Host!.GetTestServer();

    public StringBuilder LogsSb => SetupTestServerFixture.LogsSb;

    [SetUp]
    public void SetUpTestClients()
    {
        foreach (FileInfo file in new DirectoryInfo(Path.GetFullPath("fs_data/upload")).GetFiles())
            file.Delete();

        _testClient = SetupTestServerFixture.Host!.GetTestClient();
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
