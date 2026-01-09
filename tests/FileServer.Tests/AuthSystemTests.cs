using System.Net;
using System.Text;
using FileServer.Models;
using FileServer.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FileServer.Tests;

internal sealed class AuthSystemTests : TestsBase
{
    [Test]
    public async Task Auth_UsingInQuery_AntiforgeryToken_Works()
    {
        LoginResponse loginResponse = await _fsTestClient.Login();
        string queryString = $"?{Constants.AntiforgeryTokenQueryParamName}={loginResponse.AntiforgeryToken}";
        using HttpResponseMessage response = await _fsTestClient.Get(
            $"/api/files/download/file1.txt{queryString}", skipAntiforgeryTokenHeader: true);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Auth_ReportsAllProblems_WhenBothTokensAreBad()
    {
        await _fsTestClient.Login();
        _fsTestClient.CookieContainer = new();
        using HttpResponseMessage response = await _fsTestClient.Get(
            "/api/files/download/file1.txt", skipAntiforgeryTokenHeader: true);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(await GetContent(response), Is.EqualTo(
            @"""Failed to authenticate: Auth token absent. Antiforgery token absent."""));
    }

    [Test]
    public async Task Auth_WithAbsent_AntiforgeryToken_Fails()
    {
        await _fsTestClient.Login();
        using HttpResponseMessage response = await _fsTestClient.Get(
            "/api/files/download/file1.txt", skipAntiforgeryTokenHeader: true);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(await GetContent(response), Is.EqualTo(
            @"""Failed to authenticate: Antiforgery token absent."""));
    }

    [Test]
    public async Task Auth_WithAbsent_CookieToken_Fails()
    {
        await _fsTestClient.Login();
        _fsTestClient.CookieContainer = new();
        using HttpResponseMessage response = await _fsTestClient.Get("/api/files/download/file1.txt");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(await GetContent(response), Is.EqualTo(
            @"""Failed to authenticate: Auth token absent."""));
    }

    [Test]
    public async Task Auth_WithMalformed_AntiforgeryToken_Fails()
    {
        LoginResponse loginResponse = await _fsTestClient.Login();
        loginResponse.AntiforgeryToken = CreateMalformedToken();
        using HttpResponseMessage response = await _fsTestClient.Get("/api/files/download/file1.txt");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(await GetContent(response), Is.EqualTo(
            @"""Failed to authenticate: Antiforgery token not valid."""));
    }

    [Test]
    public async Task Auth_WithMalformed_CookieToken_Fails()
    {
        await _fsTestClient.Login();
        _fsTestClient.CookieContainer.SetCookies(
            _testClient.BaseAddress!,
            $"{Constants.AuthTokenCookieName}={CreateMalformedToken()}");

        using HttpResponseMessage response = await _fsTestClient.Get("/api/files/download/file1.txt");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(await GetContent(response), Is.EqualTo(
            @"""Failed to authenticate: Auth token not valid."""));
    }

    [Test]
    public async Task Auth_WithNotValid_AntiforgeryToken_Fails()
    {
        LoginResponse loginResponse = await _fsTestClient.Login();
        loginResponse.AntiforgeryToken = CreateExpiredToken(Constants.AntiforgeryClaimType);
        using HttpResponseMessage response = await _fsTestClient.Get("/api/files/download/file1.txt");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(await GetContent(response), Is.EqualTo(
            @"""Failed to authenticate: Antiforgery token not valid."""));
    }

    [Test]
    public async Task Auth_WithNotValid_CookieToken_Fails()
    {
        await _fsTestClient.Login();
        _fsTestClient.CookieContainer.SetCookies(
            _testClient.BaseAddress!,
            $"{Constants.AuthTokenCookieName}={CreateExpiredToken(Constants.AuthClaimType)}");

        using HttpResponseMessage response = await _fsTestClient.Get("/api/files/download/file1.txt");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(await GetContent(response), Is.EqualTo(
            @"""Failed to authenticate: Auth token not valid."""));
    }

    private static string CreateMalformedToken()
    {
        const string tokenJson = /*lang=json,strict*/ @"{ ""somefield"": ""111"" }";
        string tokenBase64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(tokenJson));
        return WebUtility.UrlEncode(tokenBase64String);
    }

    private static string CreateExpiredToken(string type)
    {
        TokenService tokenService = TestServer.Services.GetService<TokenService>()!;
        Token token = tokenService.CreateToken(
            new Claim()
            {
                User = Constants.MainUserName,
                Type = type,
                Expires = DateTime.UtcNow.AddSeconds(-60),
            });
        return tokenService.EncodeToken(token);
    }
}
