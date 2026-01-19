using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FileServer.Configuration;
using FileServer.Models.Auth;
using Microsoft.Extensions.Options;

namespace FileServer.Services;

internal sealed class TokenService(
    IOptionsMonitor<Settings> options)
{
    private readonly IOptionsMonitor<Settings> _options = options;

    public Token CreateToken(Claim claim) => new()
    {
        Claim = claim,
        Signature = Convert.ToBase64String(ComputeClaimSignature(claim)),
    };

    public bool TokenIsValid(Token token)
    {
        byte[] expectedSignature = ComputeClaimSignature(token.Claim);
        byte[] signature = Convert.FromBase64String(token.Signature);
        return CryptographicOperations.FixedTimeEquals(signature, expectedSignature)
            && token.Claim.Expires > DateTime.UtcNow;
    }

    public string EncodeToken(Token token)
    {
        string tokenJson = JsonSerializer.Serialize(token, Jsc.Default.Token);
        string tokenBase64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(tokenJson));
        return WebUtility.UrlEncode(tokenBase64String);
    }

    public Token? TryDecodeToken(string encodedTokenString)
    {
        try
        {
            string base64TokenString = WebUtility.UrlDecode(encodedTokenString);
            string tokenJson = Encoding.UTF8.GetString(Convert.FromBase64String(base64TokenString));
            return JsonSerializer.Deserialize(tokenJson, Jsc.Default.Token);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private byte[] ComputeClaimSignature(Claim claim)
    {
        string data = JsonSerializer.Serialize(claim, Jsc.Default.Claim);
        byte[] dataBytes = Encoding.UTF8.GetBytes(data);
        byte[] keyBytes = Encoding.UTF8.GetBytes(_options.CurrentValue.SigningKey);
        using HMACSHA256 hmac = new(keyBytes);
        return hmac.ComputeHash(dataBytes);
    }
}
