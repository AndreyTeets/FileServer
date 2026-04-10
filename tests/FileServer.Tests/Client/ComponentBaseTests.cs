using FileServer.Tests.Util;

namespace FileServer.Tests.Client;

internal sealed class ComponentBaseTests : ClientTestsBase
{
    private ComponentBaseTestsMockVDom _mockVDom;

    [SetUp]
    public void SetUp()
    {
        _mockVDom = new();
        _engine.AddHostObject("VDom", _mockVDom);
        _engine.EvalFiles([
            "wwwroot/comparer.js",
            "wwwroot/components.base.js",
        ]);

        CreateTestDerivedComponent();
    }

    [Test]
    public void Render_CachesVElem_WhenConnectedAndIdenticalProps()
    {
        _mockVDom.Connected = true;
        _engine.Eval("component.render({p: 1})");
        _engine.Eval("component.render({p: 1})");
        Assert.That(_mockVDom.CreateFakeElementCallsCount, Is.EqualTo(1));
    }

    [Test]
    public void Render_DoesntCacheVElem_WhenDifferentProps()
    {
        _mockVDom.Connected = true;
        _engine.Eval("component.render({p: 1})");
        _engine.Eval("component.render({p: 2})");
        Assert.That(_mockVDom.CreateFakeElementCallsCount, Is.EqualTo(2));
    }

    [Test]
    public void Render_DoesntCacheVElem_WhenNotConnected()
    {
        _mockVDom.Connected = false;
        _engine.Eval("component.render({p: 1})");
        _engine.Eval("component.render({p: 1})");
        Assert.That(_mockVDom.CreateFakeElementCallsCount, Is.EqualTo(2));
    }

    [Test]
    public void SetState_CallsRerender_WhenConnectedAndDifferentState()
    {
        _mockVDom.Connected = true;
        _engine.Eval("component.render()");
        _engine.Eval("component.setState({s: 2})");
        Assert.That(_mockVDom.ReplaceCallsCount, Is.EqualTo(1));
    }

    [Test]
    public void SetState_DoesntCallRerender_WhenIdenticalState()
    {
        _mockVDom.Connected = true;
        _engine.Eval("component.render()");
        _engine.Eval("component.setState({s: 1})");
        Assert.That(_mockVDom.ReplaceCallsCount, Is.Zero);
    }

    [Test]
    public void SetState_DoesntCallRerender_WhenNotConnected()
    {
        _mockVDom.Connected = false;
        _engine.Eval("component.render()");
        _engine.Eval("component.setState({s: 2})");
        Assert.That(_mockVDom.ReplaceCallsCount, Is.Zero);
    }

    private void CreateTestDerivedComponent() => _engine.Eval("""
        class TestDerivedComponent extends ComponentBase {
            constructor() { super(); }
            renderCore() { return VDom.createFakeElement(); }
        }
        const component = new TestDerivedComponent();
        component.setState({s: 1});
        """);
}

// IDE1006: Using [ScriptMember("camelCaseMemberName")] attributes is too cumbersome
// CA1515: V8ScriptEngine doesn't expose non-public members (which they all are if the type is)
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable CA1515 // Consider making public types internal
#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable MA0048 // File name must match type name

public class ComponentBaseTestsMockVDom
{
    internal bool? Connected { get; set; }
    internal int CreateFakeElementCallsCount { get; private set; } = 0;
    internal int ReplaceCallsCount { get; private set; } = 0;

    public bool isConnected(object elem) => Connected
        ?? throw new InvalidOperationException($"'{nameof(Connected)}' property is not set.");
    public object createFakeElement() => ++CreateFakeElementCallsCount;
    public void replace(object newElem, object oldElem) => ReplaceCallsCount++;
}
