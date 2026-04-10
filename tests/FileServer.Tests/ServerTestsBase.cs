using System.Text;
using FileServer.Tests.Util;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace FileServer.Tests;

internal abstract class ServerTestsBase : ILoggedTest
{
    private IHost _host;
    protected TestServer TestServer => _host.GetTestServer();
    protected HttpClient TestClient => TestServer.CreateClient();
    protected FileServerTestClient FsTestClient { get; private set; }

    public StringBuilder LogsSb { get; } = new();

    [SetUp]
    public async Task SetUpTestServer()
    {
        ClearUploadDir();
        _host = TestServerHost.Create(LogsSb);
        await _host.StartAsync();
        FsTestClient = new FileServerTestClient(TestServer.CreateHandler(), TestClient.BaseAddress);
    }

    [TearDown]
    public async Task TearDownTestServer()
    {
        FsTestClient.Dispose();
        await _host.StopAsync();
        _host.Dispose();
    }

    protected static async Task<string> GetContent(HttpResponseMessage response) =>
        await response.Content.ReadAsStringAsync();

    private static void ClearUploadDir()
    {
        foreach (FileInfo file in new DirectoryInfo(Path.GetFullPath("fs_data/upload")).GetFiles())
            file.Delete();
    }
}
