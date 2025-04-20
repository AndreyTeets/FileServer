﻿using System.Net;
using System.Text.RegularExpressions;

namespace FileServer.Tests;

internal sealed class RoutingSystemTests : TestsBase
{
    [Test]
    public async Task IndexPageRedirect_Works()
    {
        using HttpResponseMessage response = await _testClient.GetAsync("/");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(Regex.IsMatch(await GetContent(response), @"<body onload=""onDocumentLoad\(\)"">"), Is.True);
    }
}
