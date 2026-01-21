using System.Net;
using System.Text.RegularExpressions;

namespace FileServer.Tests.Routes;

internal sealed class RoutingSystemTests : TestsBase
{
    [Test]
    public async Task IndexPageRedirect_Works()
    {
        using HttpResponseMessage response = await TestClient.GetAsync("/");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(Regex.IsMatch(await GetContent(response), @"<body onload=""onDocumentLoad\(\)"">"), Is.True);
    }
}
