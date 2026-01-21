using System.Net;
using System.Net.Http.Json;
using FileServer.Models.Auth;
using Microsoft.AspNetCore.Http;

namespace FileServer.Tests.Util;

internal sealed class FileServerTestClient : IDisposable
{
#pragma warning disable CA2213 // Disposable fields should be disposed
    private readonly CookieProcessingHttpMessageHandler _cpHandler;
#pragma warning restore CA2213 // Responsibility of the HttpClient
    private readonly HttpClient _httpClient;

    public string? AntiforgeryToken { get; set; }

    public CookieContainer CookieContainer
    {
        get => _cpHandler.CookieContainer;
        set => _cpHandler.CookieContainer = value;
    }

    public FileServerTestClient(HttpMessageHandler handler, Uri? baseAddress)
    {
        _cpHandler = new(handler);
        _httpClient = new(_cpHandler);
        _httpClient.BaseAddress = baseAddress;
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }

    public async Task<HttpResponseMessage> Get(
        string requestUri,
        bool skipAntiforgeryTokenHeader = false)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, requestUri);
        if (AntiforgeryToken is not null && !skipAntiforgeryTokenHeader)
            request.Headers.Add(Constants.AntiforgeryTokenHeaderName, AntiforgeryToken);
        return await _httpClient.SendAsync(request);
    }

    public async Task<HttpResponseMessage> Post(
        string requestUri,
        HttpContent? content,
        bool skipAntiforgeryTokenHeader = false)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, requestUri);
        if (AntiforgeryToken is not null && !skipAntiforgeryTokenHeader)
            request.Headers.Add(Constants.AntiforgeryTokenHeaderName, AntiforgeryToken);
        request.Content = content;
        return await _httpClient.SendAsync(request);
    }

    public async Task<LoginResponse> Login()
    {
        using HttpRequestMessage request = new(HttpMethod.Post, "/api/auth/login");
        request.Content = JsonContent.Create(new { Password = "123456789012" });
        using HttpResponseMessage response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        LoginResponse loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>()
            ?? throw new InvalidOperationException("Invalid login response content.");
        AntiforgeryToken = loginResponse.AntiforgeryToken;
        return loginResponse;
    }

    public async Task Logout()
    {
        using HttpRequestMessage request = new(HttpMethod.Post, "/api/auth/logout");
        if (AntiforgeryToken is not null)
            request.Headers.Add(Constants.AntiforgeryTokenHeaderName, AntiforgeryToken);
        using HttpResponseMessage response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        AntiforgeryToken = null;
    }
}
