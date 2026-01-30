namespace FileServer.Tests.Util;

// IDE1006: Using [ScriptMember("camelCaseMemberName")] attributes is too cumbersome
// CA1515: V8ScriptEngine doesn't expose non-public members (which they all are if the type is)
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable CA1515 // Consider making public types internal
#pragma warning disable MA0048 // File name must match type name

public class MockDocument
{
    internal List<MockElement> Elements { get; } = [];
    internal List<string> CreateElementCalls { get; } = [];

    public MockElement createElement(string type)
    {
        MockElement elem = new(type);
        Elements.Add(elem);
        CreateElementCalls.Add(type);
        return elem;
    }
}

public class MockElement
{
    internal string Type { get; }
    internal List<MockElement> ChildElements { get; } = [];

    internal MockElement(string type)
    {
        Type = type;
    }

    public void append(MockElement elem)
    {
        ChildElements.Add(elem);
    }

    public void removeChild(MockElement oldElem)
    {
        ChildElements.Remove(oldElem);
    }

    public void replaceChild(MockElement newElem, MockElement oldElem)
    {
        int oldElemIndex = ChildElements.IndexOf(oldElem);
        ChildElements.RemoveAt(oldElemIndex);
        ChildElements.Insert(oldElemIndex, newElem);
    }
}
