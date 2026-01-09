using System.Text;
using FileServer.Tests.Util;
using Microsoft.AspNetCore.TestHost;

namespace FileServer.Tests;

#pragma warning disable CA1001 // Types that own disposable fields should be disposable
internal abstract class TestsBase : ILoggedTest
#pragma warning restore CA1001 // Remove when `dotnet format` is fixed (see https://github.com/dotnet/sdk/issues/44867)
{
#pragma warning disable CS8618 // Non-nullable variable must contain a non-null value when exiting constructor. Consider declaring it as nullable.
    private protected HttpClient _testClient;
    private protected FileServerTestClient _fsTestClient;
#pragma warning restore CS8618 // Remove when `dotnet format` is fixed (see https://github.com/dotnet/sdk/issues/44867)
    protected static TestServer TestServer => SetUpTestServerFixture.Host!.GetTestServer();

    public StringBuilder LogsSb => SetUpTestServerFixture.LogsSb;

    [SetUp]
    public void SetUpTestClients()
    {
        ClearUploadDir();
        _testClient = SetUpTestServerFixture.Host!.GetTestClient();
        _fsTestClient = new FileServerTestClient(TestServer.CreateHandler(), _testClient.BaseAddress);
    }

    [TearDown]
    public void TearDownTestClients()
    {
        _fsTestClient?.Dispose();
        _testClient?.Dispose();
    }

    protected static async Task<string> GetContent(HttpResponseMessage response) =>
        await response.Content.ReadAsStringAsync();

    private static void ClearUploadDir()
    {
        foreach (FileInfo file in new DirectoryInfo(Path.GetFullPath("fs_data/upload")).GetFiles())
            file.Delete();
    }
}
