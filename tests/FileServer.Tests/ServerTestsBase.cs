using System.Text;
using FileServer.Tests.Util;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace FileServer.Tests;

#pragma warning disable CA1001 // Types that own disposable fields should be disposable
internal abstract class ServerTestsBase : ILoggedTest
#pragma warning restore CA1001 // Remove when `dotnet format` is fixed (see https://github.com/dotnet/sdk/issues/44867)
{
#pragma warning disable CS8618 // Non-nullable variable must contain a non-null value when exiting constructor. Consider declaring it as nullable.
    private IHost _host;
    protected TestServer TestServer => _host.GetTestServer();
    protected HttpClient TestClient => TestServer.CreateClient();
    protected FileServerTestClient FsTestClient { get; private set; }
#pragma warning restore CS8618 // Remove when `dotnet format` is fixed (see https://github.com/dotnet/sdk/issues/44867)

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
