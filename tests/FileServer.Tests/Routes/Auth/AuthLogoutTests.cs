using System.Net;

namespace FileServer.Tests.Routes.Auth;

internal sealed class AuthLogoutTests : ServerTestsBase
{
    [Test]
    public async Task Logout_DeletesAuthCookie_WhenAuthorized()
    {
        await FsTestClient.Login();
        Assert.That(GetCookiesCount(), Is.Not.Zero);
        await FsTestClient.Logout();
        Assert.That(GetCookiesCount(), Is.Zero);
    }

    [Test]
    public async Task Logout_DeletesAuthCookie_WhenUnauthorized()
    {
        await FsTestClient.Login();
        Assert.That(GetCookiesCount(), Is.Not.Zero);
        await LogoutWithoutSendingAntiforgeryToken();
        Assert.That(GetCookiesCount(), Is.Zero);

        async Task LogoutWithoutSendingAntiforgeryToken()
        {
            using HttpResponseMessage response = await FsTestClient.Post(
                "/api/auth/logout", content: null, skipAntiforgeryTokenHeader: true);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(await GetContent(response), Is.EqualTo(
                @"""Failed to authenticate: Antiforgery token absent."""));
        }
    }

    private int GetCookiesCount() => FsTestClient.CookieContainer.GetAllCookies().Count;
}
