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
        LoginResponse loginReponse = await FsTestClient.Login();
        string queryString = $"?{Constants.AntiforgeryTokenQueryParamName}={loginReponse.AntiforgeryToken}";
        using HttpResponseMessage response = await FsTestClient.Get(
            $"/api/files/download/file1.txt{queryString}", skipAntiforgeryTokenHeader: true);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Auth_Requires_CookieToken()
    {
        await FsTestClient.Login();
        FsTestClient.CookieContainer = new();
        using HttpResponseMessage response = await FsTestClient.Get($"/api/files/download/file1.txt");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(await GetContent(response), Is.EqualTo(
            @"""Failed to authenticate: Auth token absent."""));
    }

    [Test]
    public async Task Auth_Requires_AntiforgeryToken()
    {
        await FsTestClient.Login();
        using HttpResponseMessage response = await FsTestClient.Get(
            $"/api/files/download/file1.txt", skipAntiforgeryTokenHeader: true);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(await GetContent(response), Is.EqualTo(
            @"""Failed to authenticate: Antiforgery token absent."""));
    }

    [Test]
    public async Task Auth_WithInvalid_CookieToken_Fails()
    {
        await FsTestClient.Login();
        FsTestClient.CookieContainer.SetCookies(
            TestClient.BaseAddress!,
            $"{Constants.AuthTokenCookieName}={CreateInvalidEncodedToken(Constants.AuthClaimType)}");

        using HttpResponseMessage response = await FsTestClient.Get($"/api/files/download/file1.txt");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(await GetContent(response), Is.EqualTo(
            @"""Failed to authenticate: Auth token not valid."""));
    }

    [Test]
    public async Task Auth_WithInvalid_AntiforgeryToken_Fails()
    {
        LoginResponse loginReponse = await FsTestClient.Login();
        loginReponse.AntiforgeryToken = CreateInvalidEncodedToken(Constants.AntiforgeryClaimType);
        using HttpResponseMessage response = await FsTestClient.Get($"/api/files/download/file1.txt");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(await GetContent(response), Is.EqualTo(
            @"""Failed to authenticate: Antiforgery token not valid."""));
    }

    [Test]
    public async Task Logout_RemovesCookieToken_WithoutError_WhenAuthorized()
    {
        await FsTestClient.Login();
        FsTestClient.CookieContainer = new();
        using HttpResponseMessage response = await FsTestClient.Get(
            "/api/files/download/file1.txt", skipAntiforgeryTokenHeader: true);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(await GetContent(response), Is.EqualTo(
            @"""Failed to authenticate: Auth token absent. Antiforgery token absent."""));
    }

    [Test]
    public async Task Logout_RemovesCookieToken_WithError_WhenUnauthorized()
    {
        {
            LoginResponse loginReponse = await FsTestClient.Login();
            using HttpResponseMessage response = await FsTestClient.Post(
                "/api/auth/logout", null, skipAntiforgeryTokenHeader: true);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(await GetContent(response), Is.EqualTo(
                @"""Failed to authenticate: Antiforgery token absent."""));
        }

        {
            using HttpResponseMessage response = await FsTestClient.Get($"/api/files/download/file1.txt");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(await GetContent(response), Is.EqualTo(
                @"""Failed to authenticate: Auth token absent."""));
        }
    }

    private string CreateInvalidEncodedToken(string type)
    {
        TokenService tokenService = TestServer.Services.GetService<TokenService>()!;
        Token token = tokenService.CreateToken(
            new Claim()
            {
                User = Constants.MainUserName,
                Type = type,
                Expires = DateTime.UtcNow.AddSeconds(-60),
            });
        string encodedToken = tokenService.EncodeToken(token);
        return encodedToken;
    }
}
