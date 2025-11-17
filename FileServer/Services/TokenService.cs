using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FileServer.Configuration;
using FileServer.Models;
using Microsoft.Extensions.Options;

namespace FileServer.Services;

public class TokenService(
    IOptionsMonitor<Settings> options)
{
    private readonly IOptionsMonitor<Settings> _options = options;

    public Token CreateToken(Claim claim) => new()
    {
        Claim = claim,
        Signature = ComputeClaimSignature(claim),
    };

    public bool TokenIsValid(Token token)
    {
        string signature = ComputeClaimSignature(token.Claim!);
        if (token.Signature != signature)
            return false;
        if (DateTime.UtcNow > token.Claim!.Expires!)
            return false;
        return true;
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

    private string ComputeClaimSignature(Claim claim)
    {
        string data = JsonSerializer.Serialize(claim, Jsc.Default.Claim);
        byte[] dataBytes = Encoding.UTF8.GetBytes(data);
        byte[] keyBytes = Encoding.UTF8.GetBytes(_options.CurrentValue.SigningKey!);
        using HMACSHA256 hmac = new(keyBytes);
        byte[] signatureBytes = hmac.ComputeHash(dataBytes);
        return Convert.ToBase64String(signatureBytes);
    }
}
