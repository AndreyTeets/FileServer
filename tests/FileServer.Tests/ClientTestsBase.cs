using FileServer.Tests.Util;
using Microsoft.ClearScript.V8;

namespace FileServer.Tests;

[Category("Client")]
internal abstract class ClientTestsBase
{
    protected V8ScriptEngine _engine;
    protected MockDocument _mockDocument;

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
