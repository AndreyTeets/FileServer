using FileServer.Tests.Util;
using Microsoft.ClearScript;

namespace FileServer.Tests.Client;

internal sealed class ComparerTests : ClientTestsBase
{
    [SetUp]
    public void SetUp()
    {
        _engine.EvalFiles([
            "wwwroot/comparer.js",
        ]);
    }

    [TestCase(/*lang=json*/ "{a:1}", /*lang=json*/ "{}")]
    [TestCase(/*lang=json*/ "{a:1}", /*lang=json*/ "{a:undefined}")]
    [TestCase(/*lang=json*/ "{a:1}", /*lang=json*/ "{a:null}")]
    [TestCase(/*lang=json*/ "{a:1}", /*lang=json*/ "{a:'1'}")]
    [TestCase(/*lang=json*/ "{a:1}", /*lang=json*/ "{a:2}")]
    [TestCase(/*lang=json*/ "{a:1,b:{c:'1'}}", /*lang=json*/ "{a:1,b:{}}")]
    [TestCase(/*lang=json*/ "{a:1,b:{c:'1'}}", /*lang=json*/ "{a:1,b:{c:undefined}}")]
    [TestCase(/*lang=json*/ "{a:1,b:{c:'1'}}", /*lang=json*/ "{a:1,b:{c:null}}")]
    [TestCase(/*lang=json*/ "{a:1,b:{c:'1'}}", /*lang=json*/ "{a:1,b:{c:1}}")]
    [TestCase(/*lang=json*/ "{a:1,b:{c:'1'}}", /*lang=json*/ "{a:1,b:{c:'2'}}")]
    [TestCase(/*lang=json*/ "{a:[1,2]}", /*lang=json*/ "{a:[1]}")]
    [TestCase(/*lang=json*/ "{a:[1,2]}", /*lang=json*/ "{a:[1,undefined]}")]
    [TestCase(/*lang=json*/ "{a:[1,2]}", /*lang=json*/ "{a:[1,null]}")]
    [TestCase(/*lang=json*/ "{a:[1,2]}", /*lang=json*/ "{a:[1,'2']}")]
    [TestCase(/*lang=json*/ "{a:[1,2]}", /*lang=json*/ "{a:[1,3]}")]
    [TestCase(/*lang=json*/ "{a:[1,2]}", /*lang=json*/ "{a:[2,1]}")]
    public async Task ObjectsAreIdentical_ReturnsFalse_WhenDifferent(string obj1, string obj2)
    {
        Assert.That(_engine.Eval<bool>($"""
            Comparer.objectsAreIdentical({obj1}, {obj2})
            """), Is.False);

        Assert.That(_engine.Eval<bool>($"""
            Comparer.objectsAreIdentical({obj2}, {obj1})
            """), Is.False);
    }

    [TestCase(/*lang=json*/ "{a:1}", /*lang=json*/ "{a:1}")]
    [TestCase(/*lang=json*/ "{a:1,b:{x:{},y:'1'}}", /*lang=json*/ "{b:{y:'1',x:{}},a:1}")]
    [TestCase(/*lang=json*/ "[{a:[1,2]},{b:['1','2']}]", /*lang=json*/ "[{a:[1,2]},{b:['1','2']}]")]
    [TestCase(/*lang=json*/ "[undefined,null,1,'1',[]]", /*lang=json*/ "[undefined,null,1,'1',[]]")]
    public async Task ObjectsAreIdentical_ReturnsTrue_WhenIdentical(string obj1, string obj2)
    {
        Assert.That(_engine.Eval<bool>($"""
            Comparer.objectsAreIdentical({obj1}, {obj2})
            """), Is.True);

        Assert.That(_engine.Eval<bool>($"""
            Comparer.objectsAreIdentical({obj2}, {obj1})
            """), Is.True);
    }

    [Test]
    public async Task ObjectsAreIdentical_ComparesFunctionsByReference()
    {
        Assert.That(_engine.Eval<bool>("""
            const func = () => { return "123" };
            const func1 = func;
            const func2 = () => { return "123" };
            Comparer.objectsAreIdentical({a: func}, {a: func1})
            """), Is.True);

        Assert.That(_engine.Eval<bool>("""
            Comparer.objectsAreIdentical({a: func}, {a: func2})
            """), Is.False);
    }

    [Test]
    public async Task ObjectsAreIdentical_IgnoresExcludedProps()
    {
        const string obj1 = /*lang=json*/ "{a:1,b:1,c:{x:1,y:1}}";
        const string obj2 = /*lang=json*/ "{a:1,b:2,c:{x:1,y:2}}";

        Assert.That(_engine.Eval<bool>($"""
            Comparer.objectsAreIdentical({obj1}, {obj2}, ["b", "y"])
            """), Is.True);
    }

    [Test]
    public async Task ObjectsAreIdentical_Throws_WhenNonObject()
    {
        ScriptEngineException? ex = Assert.Throws<ScriptEngineException>(() =>
        {
            _engine.Eval("""
                Comparer.objectsAreIdentical(1, 1)
                """);
        });
        Assert.That(ex.Message, Is.EqualTo("Error: Non-object type 'number'/'number' while comparing objects."));
    }
}
