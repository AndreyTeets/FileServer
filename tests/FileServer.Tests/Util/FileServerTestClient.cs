using System.Net;
using System.Net.Http.Json;
using FileServer.Models;
using Microsoft.AspNetCore.Http;

namespace FileServer.Tests.Util;

internal sealed class FileServerTestClient : IDisposable
{
#pragma warning disable CA2213 // Disposable fields should be disposed
    private readonly CookieProcessingHttpMessageHandler _cpHandler;
#pragma warning restore CA2213 // Responsibility of the HttpClient
    private readonly HttpClient _httpClient;
    private LoginResponse? _loginResponse;

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
        if (_loginResponse is not null && !skipAntiforgeryTokenHeader)
            request.Headers.Add(Constants.AntiforgeryTokenHeaderName, _loginResponse.AntiforgeryToken);
        return await _httpClient.SendAsync(request);
    }

    public async Task<HttpResponseMessage> Post(
        string requestUri,
        HttpContent? content,
        bool skipAntiforgeryTokenHeader = false)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, requestUri);
        if (_loginResponse is not null && !skipAntiforgeryTokenHeader)
            request.Headers.Add(Constants.AntiforgeryTokenHeaderName, _loginResponse.AntiforgeryToken);
        request.Content = content;
        return await _httpClient.SendAsync(request);
    }

    public async Task<LoginResponse> Login()
    {
        using HttpRequestMessage request = new(HttpMethod.Post, "/api/auth/login");
        request.Content = JsonContent.Create(new { Password = "123456789012" });
        using HttpResponseMessage response = await _httpClient.SendAsync(request);
        _loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(_loginResponse, Is.Not.Null);
        return _loginResponse!;
    }

    public async Task Logout()
    {
        using HttpRequestMessage request = new(HttpMethod.Post, "/api/auth/logout");
        if (_loginResponse is not null)
            request.Headers.Add(Constants.AntiforgeryTokenHeaderName, _loginResponse.AntiforgeryToken);
        using HttpResponseMessage response = await _httpClient.SendAsync(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}
