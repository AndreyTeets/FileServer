using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using FileServer.Models;
using FileServer.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FileServer.Tests.Routes;

internal sealed class AuthRoutesTests : TestsBase
{
    [Test]
    public async Task Login_ProducesCorrectResponse_WhenSuccessful()
    {
        LoginResponse loginResponse = await _fsTestClient.Login();

        Assert.That(loginResponse, Is.Not.Null);
        Assert.That(loginResponse.LoginInfo, Is.Not.Null);
        Assert.That(loginResponse.LoginInfo.User, Is.EqualTo(Constants.MainUserName));
        Assert.That(loginResponse.LoginInfo.TokensExpire, Is.GreaterThan(DateTime.UtcNow));
        Assert.That(loginResponse.AntiforgeryToken, Is.Not.Null.And.Not.Empty);
        AssertThatTokenIsCorrectAndValid(loginResponse.AntiforgeryToken, Constants.AntiforgeryClaimType);

        Cookie? authCookie = _fsTestClient.CookieContainer.GetAllCookies()
            .SingleOrDefault(x => x.Name == Constants.AuthTokenCookieName);
        Assert.That(authCookie, Is.Not.Null);
        Assert.That(authCookie.Expired, Is.False);
        Assert.That(authCookie.Value, Is.Not.Null.And.Not.Empty);
        AssertThatTokenIsCorrectAndValid(WebUtility.UrlDecode(authCookie.Value), Constants.AuthClaimType);

        void AssertThatTokenIsCorrectAndValid(string encodedTokenString, string tokenType)
        {
            TokenService tokenService = TestServer.Services.GetRequiredService<TokenService>();
            Token? token = tokenService.TryDecodeToken(encodedTokenString);
            Assert.That(token, Is.Not.Null);
            Assert.That(token.Claim?.User, Is.EqualTo(Constants.MainUserName));
            Assert.That(token.Claim.Type, Is.EqualTo(tokenType));
            Assert.That(token.Claim.Expires, Is.EqualTo(loginResponse.LoginInfo.TokensExpire));
            Assert.That(tokenService.TokenIsValid(token), Is.True);
        }
    }

    [TestCase(/*lang=json,strict*/ null, HttpStatusCode.BadRequest)]
    [TestCase(/*lang=json,strict*/ "", HttpStatusCode.BadRequest)]
    [TestCase(/*lang=json,strict*/ "{}", HttpStatusCode.Unauthorized)]
    [TestCase(/*lang=json,strict*/ @"{ ""SomeField"": ""111"" }", HttpStatusCode.Unauthorized)]
    [TestCase(/*lang=json,strict*/ @"{ ""Password"": 111 }", HttpStatusCode.BadRequest)]
    [TestCase(/*lang=json,strict*/ @"{ ""Password"": { ""SomeField"": ""111"" } }", HttpStatusCode.BadRequest)]
    [TestCase(/*lang=json,strict*/ @"{ ""Password"": null }", HttpStatusCode.Unauthorized)]
    [TestCase(/*lang=json,strict*/ @"{ ""Password"": ""111"" }", HttpStatusCode.Unauthorized)]
    public async Task Login_Fails_OnWrongRequest(
        string? requestBodyJsonStr, HttpStatusCode expectedResponseStatusCode)
    {
        using HttpResponseMessage response = await _fsTestClient.Post("/api/auth/login",
            content: CreateJsonContent(requestBodyJsonStr));
        Assert.That(response.StatusCode, Is.EqualTo(expectedResponseStatusCode));

        static StringContent? CreateJsonContent(string? jsonStr) => jsonStr is not null
            ? new(jsonStr, MediaTypeHeaderValue.Parse(MediaTypeNames.Application.Json))
            : null;
    }

    [Test]
    public async Task Logout_DeletesAuthCookie_WhenAuthorized()
    {
        await _fsTestClient.Login();
        Assert.That(GetCookiesCount(), Is.Not.Zero);
        await _fsTestClient.Logout();
        Assert.That(GetCookiesCount(), Is.Zero);
    }

    [Test]
    public async Task Logout_DeletesAuthCookie_WhenUnauthorized()
    {
        await _fsTestClient.Login();
        Assert.That(GetCookiesCount(), Is.Not.Zero);
        await LogoutWithoutSendingAntiforgeryToken();
        Assert.That(GetCookiesCount(), Is.Zero);

        async Task LogoutWithoutSendingAntiforgeryToken()
        {
            using HttpResponseMessage response = await _fsTestClient.Post(
                "/api/auth/logout", content: null, skipAntiforgeryTokenHeader: true);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(await GetContent(response), Is.EqualTo(
                @"""Failed to authenticate: Antiforgery token absent."""));
        }
    }

    private int GetCookiesCount() => _fsTestClient.CookieContainer.GetAllCookies().Count;
}
