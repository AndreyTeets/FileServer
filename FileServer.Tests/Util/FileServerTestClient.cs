using System.Net;
using System.Net.Http.Json;
using FileServer.Models;
using Microsoft.AspNetCore.Http;

namespace FileServer.Tests.Util;

public class FileServerTestClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly CookieProcessingHttpMessageHandler _handler;
    private LoginResponse? _loginResponse;

    public CookieContainer CookieContainer
    {
        get => _handler.CookieContainer; set => _handler.CookieContainer = value;
    }

    public FileServerTestClient(HttpClient httpClient, CookieProcessingHttpMessageHandler handler)
    {
        _httpClient = httpClient;
        _handler = handler;
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
        HttpResponseMessage response = await _httpClient.SendAsync(request);
        return response;
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
        HttpResponseMessage response = await _httpClient.SendAsync(request);
        return response;
    }

    public async Task<LoginResponse> Login()
    {
        using HttpRequestMessage request = new(HttpMethod.Post, "/api/auth/login");
        request.Content = JsonContent.Create(new { Password = "012345678912" });
        using HttpResponseMessage response = await _httpClient.SendAsync(request);
        _loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(_loginResponse, Is.Not.Null);
        return _loginResponse;
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
