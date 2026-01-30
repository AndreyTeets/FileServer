using FileServer.Tests.Util;
using Microsoft.ClearScript.V8;

namespace FileServer.Tests;

#pragma warning disable CA1001 // Types that own disposable fields should be disposable
[Category("Client")]
internal abstract class ClientTestsBase
#pragma warning restore CA1001 // Remove when `dotnet format` is fixed (see https://github.com/dotnet/sdk/issues/44867)
{
#pragma warning disable CS8618 // Non-nullable variable must contain a non-null value when exiting constructor. Consider declaring it as nullable.
    protected V8ScriptEngine _engine;
    protected MockDocument _mockDocument;
#pragma warning restore CS8618 // Remove when `dotnet format` is fixed (see https://github.com/dotnet/sdk/issues/44867)

    [SetUp]
    public void SetUpJavaScriptEnvironment()
    {
        _engine = new();
        _mockDocument = new();
    }

    [TearDown]
    public void TearDownJavaScriptEnvironment()
    {
        _engine.Dispose();
    }
}
