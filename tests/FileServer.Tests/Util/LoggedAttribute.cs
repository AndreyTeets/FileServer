using System.Text;
using NUnit.Framework.Interfaces;

namespace FileServer.Tests.Util;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
internal sealed class LoggedAttribute : Attribute, ITestAction
{
    public ActionTargets Targets => ActionTargets.Test;

    public void BeforeTest(ITest test)
    {
        if (test.Fixture is ILoggedTest loggedTest)
            loggedTest.LogsSb.Clear();
        TestContext.Out.WriteLine($"------Starting {test.FullName}");
    }

    public void AfterTest(ITest test)
    {
        if (test.Fixture is ILoggedTest loggedTest)
            TestContext.Out.Write(loggedTest.LogsSb.ToString());
        TestContext.Out.WriteLine($"------Finished {test.FullName}");
    }
}

#pragma warning disable MA0048 // File name must match type name
[Logged]
internal interface ILoggedTest
{
    public StringBuilder LogsSb { get; }
}
