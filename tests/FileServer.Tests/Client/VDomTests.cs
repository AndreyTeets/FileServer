using System.Text.Json.Nodes;
using FileServer.Tests.Util;

namespace FileServer.Tests.Client;

internal sealed class VDomTests : ClientTestsBase
{
    [SetUp]
    public void SetUp()
    {
        _engine.AddHostObject("document", _mockDocument);
        _engine.EvalFiles([
            "wwwroot/comparer.js",
            "wwwroot/vdom.js",
        ]);
    }

    [TestCase(false, false, false)]
    [TestCase(false, true, false)]
    [TestCase(true, false, false)]
    [TestCase(true, true, true)]
    public async Task IsConnected_ReturnsCorrectResult(bool rootElemIsSet, bool elemParentIsSet, bool expectedResult)
    {
        _engine.Eval("""
            const vRoot = VDom.createElement("does_not_matter");
            const elem = VDom.createElement("does_not_matter");
            """);
        if (rootElemIsSet)
            _engine.Eval("VDom.rootElem = vRoot");
        if (elemParentIsSet)
            _engine.Eval("vRoot.append(elem)");
        Assert.That(_engine.Eval<bool>("VDom.isConnected(elem)"), Is.EqualTo(expectedResult));
    }

    [TestCase(/*lang=json*/ "{a:{}}", /*lang=json*/ "{a:{}}", "a", "a", 0)]
    [TestCase(/*lang=json*/ "{a:{}}", /*lang=json*/ "{a:{}}", "app", "app", 0)]
    [TestCase(/*lang=json*/ "{a:{}}", /*lang=json*/ "{A:{}}", "a", "A", 1)]
    [TestCase(/*lang=json*/ "{a:{}}", /*lang=json*/ "{A:{}}", "app", "app", 1)]
    [TestCase(/*lang=json*/ "{a:{}}", /*lang=json*/ "{A:{},b:{}}", "app", "app", 3)]
    [TestCase(/*lang=json*/ "{a:{}}", /*lang=json*/ "{b:{},A:{}}", "app", "app", 3)]
    [TestCase(/*lang=json*/ "{a:{}}", /*lang=json*/ "{a:{},b:{}}", "app", "app", 1)]
    [TestCase(/*lang=json*/ "{a:{}}", /*lang=json*/ "{b:{},a:{}}", "app", "app", 3)]
    [TestCase(/*lang=json*/ "{a:{}}", /*lang=json*/ "{a:{},b:{},c:{}}", "app", "app", 4)]
    [TestCase(/*lang=json*/ "{a:{}}", /*lang=json*/ "{b:{},a:{},c:{}}", "app", "app", 4)]
    [TestCase(/*lang=json*/ "{a:{}}", /*lang=json*/ "{b:{},c:{},a:{}}", "app", "app", 4)]
    [TestCase(/*lang=json*/ "{a:{}}", /*lang=json*/ "{}", "app", "app", 0)]
    [TestCase(/*lang=json*/ "{a:{},b:{}}", /*lang=json*/ "{b:{},a:{}}", "app", "app", 3)]
    [TestCase(/*lang=json*/ "{a:{},b:{}}", /*lang=json*/ "{A:{},b:{}}", "app", "app", 1)]
    [TestCase(/*lang=json*/ "{a:{},b:{}}", /*lang=json*/ "{a:{},B:{}}", "app", "app", 1)]
    [TestCase(/*lang=json*/ "{a:{},b:{}}", /*lang=json*/ "{A:{},B:{}}", "app", "app", 3)]
    [TestCase(/*lang=json*/ "{a:{},b:{}}", /*lang=json*/ "{a:{},b:{},c:{}}", "app", "app", 1)]
    [TestCase(/*lang=json*/ "{a:{},b:{}}", /*lang=json*/ "{a:{},c:{},b:{}}", "app", "app", 4)]
    [TestCase(/*lang=json*/ "{a:{},b:{}}", /*lang=json*/ "{c:{},a:{},b:{}}", "app", "app", 4)]
    [TestCase(/*lang=json*/ "{a:{},b:{}}", /*lang=json*/ "{A:{}}", "app", "app", 2)]
    [TestCase(/*lang=json*/ "{a:{},b:{}}", /*lang=json*/ "{B:{}}", "app", "app", 2)]
    [TestCase(/*lang=json*/ "{a:{},b:{}}", /*lang=json*/ "{a:{}}", "app", "app", 0)]
    [TestCase(/*lang=json*/ "{a:{},b:{}}", /*lang=json*/ "{b:{}}", "app", "app", 2)]
    [TestCase(/*lang=json*/ "{a:{},b:{}}", /*lang=json*/ "{}", "app", "app", 1)]
    [TestCase(/*lang=json*/ "{a:{},b:{},c:{}}", /*lang=json*/ "{a:{},B:{},c:{}}", "app", "app", 1)]
    [TestCase(/*lang=json*/ "{a:{},b:{},c:{}}", /*lang=json*/ "{a:{},c:{}}", "app", "app", 3)]
    [TestCase(/*lang=json*/ "{a:{},b:{},c:{}}", /*lang=json*/ "{a:{}}", "app", "app", 2)]
    [TestCase(/*lang=json*/ "{a:{},b:{},c:{}}", /*lang=json*/ "{b:{}}", "app", "app", 2)]
    [TestCase(/*lang=json*/ "{a:{},b:{},c:{}}", /*lang=json*/ "{c:{}}", "app", "app", 2)]
    [TestCase(/*lang=json*/ "{a:{x:{}}}", /*lang=json*/ "{a:{X:{}}}", "x", "X", 1)]
    [TestCase(/*lang=json*/ "{a:{x:{}}}", /*lang=json*/ "{a:{X:{}}}", "app", "app", 1)]
    [TestCase(/*lang=json*/ "{a:{x:{}}}", /*lang=json*/ "{a:{X:{},y:{}}}", "app", "app", 3)]
    [TestCase(/*lang=json*/ "{a:{x:{}}}", /*lang=json*/ "{a:{y:{},X:{}}}", "app", "app", 3)]
    [TestCase(/*lang=json*/ "{a:{x:{}}}", /*lang=json*/ "{a:{X:{}},b:{}}", "app", "app", 4)]
    [TestCase(/*lang=json*/ "{a:{x:{}}}", /*lang=json*/ "{b:{},a:{X:{}}}", "app", "app", 4)]
    [TestCase(/*lang=json*/ "{a:{x:{}}}", /*lang=json*/ "{a:{x:{},y:{}}}", "app", "app", 1)]
    [TestCase(/*lang=json*/ "{a:{x:{}}}", /*lang=json*/ "{a:{y:{},x:{}}}", "app", "app", 3)]
    [TestCase(/*lang=json*/ "{a:{x:{}}}", /*lang=json*/ "{a:{x:{},y:{},z:{}}}", "app", "app", 4)]
    [TestCase(/*lang=json*/ "{a:{x:{}}}", /*lang=json*/ "{a:{y:{},x:{},z:{}}}", "app", "app", 4)]
    [TestCase(/*lang=json*/ "{a:{x:{}}}", /*lang=json*/ "{a:{y:{},z:{},x:{}}}", "app", "app", 4)]
    [TestCase(/*lang=json*/ "{a:{x:{}}}", /*lang=json*/ "{a:{}}", "app", "app", 0)]
    [TestCase(/*lang=json*/ "{a:{x:{},y:{}}}", /*lang=json*/ "{a:{y:{},x:{}}}", "app", "app", 3)]
    [TestCase(/*lang=json*/ "{a:{x:{},y:{}}}", /*lang=json*/ "{a:{X:{},y:{}}}", "app", "app", 1)]
    [TestCase(/*lang=json*/ "{a:{x:{},y:{}}}", /*lang=json*/ "{a:{x:{},Y:{}}}", "app", "app", 1)]
    [TestCase(/*lang=json*/ "{a:{x:{},y:{}}}", /*lang=json*/ "{a:{X:{},Y:{}}}", "app", "app", 3)]
    [TestCase(/*lang=json*/ "{a:{x:{},y:{}}}", /*lang=json*/ "{a:{x:{},y:{},z:{}}}", "app", "app", 1)]
    [TestCase(/*lang=json*/ "{a:{x:{},y:{}}}", /*lang=json*/ "{a:{x:{},z:{},y:{}}}", "app", "app", 4)]
    [TestCase(/*lang=json*/ "{a:{x:{},y:{}}}", /*lang=json*/ "{a:{z:{},x:{},y:{}}}", "app", "app", 4)]
    [TestCase(/*lang=json*/ "{a:{x:{},y:{}}}", /*lang=json*/ "{a:{X:{}}}", "app", "app", 2)]
    [TestCase(/*lang=json*/ "{a:{x:{},y:{}}}", /*lang=json*/ "{a:{Y:{}}}", "app", "app", 2)]
    [TestCase(/*lang=json*/ "{a:{x:{},y:{}}}", /*lang=json*/ "{a:{x:{}}}", "app", "app", 0)]
    [TestCase(/*lang=json*/ "{a:{x:{},y:{}}}", /*lang=json*/ "{a:{y:{}}}", "app", "app", 2)]
    [TestCase(/*lang=json*/ "{a:{x:{},y:{}}}", /*lang=json*/ "{a:{}}", "app", "app", 1)]
    [TestCase(/*lang=json*/ "{a:{x:{},y:{},z:{}}}", /*lang=json*/ "{a:{x:{},Y:{},z:{}}}", "app", "app", 1)]
    [TestCase(/*lang=json*/ "{a:{x:{},y:{},z:{}}}", /*lang=json*/ "{a:{x:{},z:{}}}", "app", "app", 3)]
    [TestCase(/*lang=json*/ "{a:{x:{},y:{},z:{}}}", /*lang=json*/ "{a:{x:{}}}", "app", "app", 2)]
    [TestCase(/*lang=json*/ "{a:{x:{},y:{},z:{}}}", /*lang=json*/ "{a:{y:{}}}", "app", "app", 2)]
    [TestCase(/*lang=json*/ "{a:{x:{},y:{},z:{}}}", /*lang=json*/ "{a:{z:{}}}", "app", "app", 2)]
    public async Task Replace_ChangesVDomAndDomCorrectly(
        string oldAppTree,
        string newAppTree,
        string oldElem,
        string newElem,
        int expectedDomElemCreations)
    {
        CreateVElements(oldAppTree, newAppTree, oldElem, newElem);
        _engine.Eval("vRoot.setDomElem(vRoot.createDomElem())"); // Create DOM tree for old vElements
        _mockDocument.CreateElementCalls.Clear();

        MockElement root = _mockDocument.Elements[0];
        Assert.That(GetDomTree(root), Is.EqualTo($"{{app:{oldAppTree}}}"));

        _engine.Eval("VDom.replace(newElem, oldElem)");
        EnsureAllReplacedNewVElementsAreConnectedAndHaveDomElemSet();
        Assert.That(_mockDocument.CreateElementCalls, Has.Count.EqualTo(expectedDomElemCreations));
        Assert.That(GetDomTree(root), Is.EqualTo($"{{app:{newAppTree}}}"));

        void CreateVElements(string oldAppTree, string newAppTree, string oldElem, string newElem) => _engine.Eval($$"""
            function appendChildrenRecursive(elem, elemTree, appendedElements) {
                for (const key of Object.keys(elemTree)) {
                    const childName = `${key}_elem`;
                    const childTree = elemTree[key];
                    const child = VDom.createElement(childName);
                    elem.append(child);
                    appendedElements[childName] = child;
                    appendChildrenRecursive(child, childTree, appendedElements);
                }
            }
            const vRoot = VDom.createElement("root_elem");
            const fRoot = VDom.createElement("fake_root_elem_to_pass_to_append_func");
            const allOldElements = {};
            const allNewElements = {};
            appendChildrenRecursive(vRoot, {app: {{oldAppTree}}}, allOldElements);
            appendChildrenRecursive(fRoot, {app: {{newAppTree}}}, allNewElements);
            const oldElem = allOldElements[`{{oldElem}}_elem`];
            const newElem = allNewElements[`{{newElem}}_elem`];
            """);

        void EnsureAllReplacedNewVElementsAreConnectedAndHaveDomElemSet() => _engine.Eval("""
            VDom.rootElem = vRoot;
            function isReplacedNewVElement(elem) { // Max depth in test data is limited - root->app->abc->xyz
                return elem === newElem || elem.vParent === newElem || elem.vParent?.vParent === newElem
            }
            for (const elem of Object.values(allNewElements)) {
                if (!isReplacedNewVElement(elem))
                    continue;
                if (!VDom.isConnected(elem))
                    throw new Error("Element not connected.");
                elem.getDomElem(); // Will throw if DOM element is not set
            }
            """);

        static string GetDomTree(MockElement elem)
        {
            return AddChildrenRecursive(elem).ToJsonString().Replace("\"", "");
            static JsonObject AddChildrenRecursive(MockElement elem)
            {
                JsonObject res = [];
                foreach (MockElement child in elem.ChildElements)
                    res[child.Type[0..^5]] = AddChildrenRecursive(child);
                return res;
            }
        }
    }
}
