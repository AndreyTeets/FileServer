using System.Net;

namespace FileServer.Tests.Routes.Auth;

internal sealed class AuthLogoutTests : TestsBase
{
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
