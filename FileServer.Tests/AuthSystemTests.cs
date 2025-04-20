using System.Net;
using FileServer.Models;
using FileServer.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FileServer.Tests;

public class AuthSystemTests : TestsBase
{
    [Test]
    public async Task AntiforgeryToken_UsingQueryString_Works()
    {
        LoginResponse loginReponse = await _fsTestClient.Login();
        string queryString = $"?{Constants.AntiforgeryTokenQueryParamName}={loginReponse.AntiforgeryToken}";
        using HttpResponseMessage response = await _fsTestClient.Get(
            $"/api/files/download/file1.txt{queryString}", skipAntiforgeryTokenHeader: true);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Auth_Requires_CookieToken()
    {
        await _fsTestClient.Login();
        _fsTestClient.CookieContainer = new();
        using HttpResponseMessage response = await _fsTestClient.Get("/api/files/download/file1.txt");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(await GetContent(response), Is.EqualTo(
            @"""Failed to authenticate: Auth token absent."""));
    }

    [Test]
    public async Task Auth_Requires_AntiforgeryToken()
    {
        await _fsTestClient.Login();
        using HttpResponseMessage response = await _fsTestClient.Get(
            "/api/files/download/file1.txt", skipAntiforgeryTokenHeader: true);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(await GetContent(response), Is.EqualTo(
            @"""Failed to authenticate: Antiforgery token absent."""));
    }

    [Test]
    public async Task Auth_WithInvalid_CookieToken_Fails()
    {
        await _fsTestClient.Login();
        _fsTestClient.CookieContainer.SetCookies(
            _testClient.BaseAddress!,
            $"{Constants.AuthTokenCookieName}={CreateInvalidEncodedToken(Constants.AuthClaimType)}");

        using HttpResponseMessage response = await _fsTestClient.Get("/api/files/download/file1.txt");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(await GetContent(response), Is.EqualTo(
            @"""Failed to authenticate: Auth token not valid."""));
    }

    [Test]
    public async Task Auth_WithInvalid_AntiforgeryToken_Fails()
    {
        LoginResponse loginReponse = await _fsTestClient.Login();
        loginReponse.AntiforgeryToken = CreateInvalidEncodedToken(Constants.AntiforgeryClaimType);
        using HttpResponseMessage response = await _fsTestClient.Get("/api/files/download/file1.txt");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(await GetContent(response), Is.EqualTo(
            @"""Failed to authenticate: Antiforgery token not valid."""));
    }

    [Test]
    public async Task Logout_RemovesCookieToken_WithoutError_WhenAuthorized()
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
    public async Task Logout_RemovesCookieToken_WithError_WhenUnauthorized()
    {
        await Login();
        await GetFileThatRequiresAuth();

        async Task Login()
        {
            await _fsTestClient.Login();
            using HttpResponseMessage response = await _fsTestClient.Post(
                "/api/auth/logout", content: null, skipAntiforgeryTokenHeader: true);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(await GetContent(response), Is.EqualTo(
                @"""Failed to authenticate: Antiforgery token absent."""));
        }

        async Task GetFileThatRequiresAuth()
        {
            using HttpResponseMessage response = await _fsTestClient.Get("/api/files/download/file1.txt");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(await GetContent(response), Is.EqualTo(
                @"""Failed to authenticate: Auth token absent."""));
        }
    }

    private static string CreateInvalidEncodedToken(string type)
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
