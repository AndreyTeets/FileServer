using System.Net;
using System.Text;
using FileServer.Models;
using FileServer.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FileServer.Tests.Services;

internal sealed class TokenServiceTests : TestsBase
{
#pragma warning disable CS8618 // Non-nullable variable must contain a non-null value when exiting constructor. Consider declaring it as nullable.
    private TokenService _tokenService;
#pragma warning restore CS8618 // Remove when `dotnet format` is fixed (see https://github.com/dotnet/sdk/issues/44867)

    [SetUp]
    public void SetUp()
    {
        _tokenService = TestServer.Services.GetRequiredService<TokenService>();
    }

    [Test]
    public async Task Decode_EncodedToken_IsEquivalentToOriginal()
    {
        Token token = _tokenService.CreateToken(CreateTestClaim());
        string encodedTokenString = _tokenService.EncodeToken(token);
        Token? decodedToken = _tokenService.TryDecodeToken(encodedTokenString);

        Assert.That(decodedToken, Is.Not.Null);
        Assert.That(decodedToken.Claim, Is.Not.Null);
        Assert.That(decodedToken.Claim.User, Is.EqualTo(token.Claim.User));
        Assert.That(decodedToken.Claim.Type, Is.EqualTo(token.Claim.Type));
        Assert.That(decodedToken.Claim.Expires, Is.EqualTo(token.Claim.Expires));
        Assert.That(decodedToken.Signature, Is.EqualTo(token.Signature));
    }

    [Test]
    public async Task Decode_ReturnsCorrectToken_ForProperInput()
    {
        const string tokenJson = /*lang=json,strict*/
            @"{ ""claim"": { ""user"": ""111"", ""type"": ""111"", ""expires"": ""2026-01-01T22:33:44.5556667Z"" }, ""signature"": ""111"" }";
        Token? token = _tokenService.TryDecodeToken(EncodeTokenJson(tokenJson));

        Assert.That(token, Is.Not.Null);
        Assert.That(token.Claim, Is.Not.Null);
        Assert.That(token.Claim.User, Is.EqualTo("111"));
        Assert.That(token.Claim.Type, Is.EqualTo("111"));
        Assert.That(token.Claim.Expires.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"), Is.EqualTo("2026-01-01T22:33:44.5556667Z"));
        Assert.That(token.Signature, Is.EqualTo("111"));
    }

    [TestCase(/*lang=json,strict*/ "")]
    [TestCase(/*lang=json,strict*/ "{}")]
    [TestCase(/*lang=json,strict*/ @"{ ""somefield"": ""111"" }")]
    [TestCase(/*lang=json,strict*/ @"{ ""signature"": ""111"" }")]
    [TestCase(/*lang=json,strict*/ @"{ ""claim"": { }, ""signature"": ""111"" }")]
    [TestCase(/*lang=json,strict*/ @"{ ""claim"": { ""user"": ""111"" }, ""signature"": ""111"" }")]
    public async Task Decode_ReturnsNull_ForMalformedInput(string tokenJson)
    {
        Token? token = _tokenService.TryDecodeToken(EncodeTokenJson(tokenJson));
        Assert.That(token, Is.Null);
    }

    [Test]
    public async Task IsValid_ReturnsFalse_WhenBadSignature()
    {
        Token token = new() { Claim = CreateTestClaim(), Signature = "some_bad_signature" };
        bool isValid = _tokenService.TokenIsValid(token);
        Assert.That(isValid, Is.False);
    }

    [Test]
    public async Task IsValid_ReturnsFalse_WhenExpired()
    {
        Token token = _tokenService.CreateToken(CreateTestClaim(expired: true));
        bool isValid = _tokenService.TokenIsValid(token);
        Assert.That(isValid, Is.False);
    }

    [Test]
    public async Task IsValid_ReturnsTrue_WhenNormallyCreated()
    {
        Token token = _tokenService.CreateToken(CreateTestClaim());
        bool isValid = _tokenService.TokenIsValid(token);
        Assert.That(isValid, Is.True);
    }

    private static string EncodeTokenJson(string tokenJson)
    {
        string tokenBase64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(tokenJson));
        return WebUtility.UrlEncode(tokenBase64String);
    }

    private static Claim CreateTestClaim(bool expired = false) => new()
    {
        User = "some_user_name",
        Type = "some_token_type",
        Expires = DateTime.UtcNow.AddSeconds(expired ? -60 : 60),
    };
}
