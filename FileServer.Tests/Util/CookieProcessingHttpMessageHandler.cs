using System.Net;
using Microsoft.Net.Http.Headers;

namespace FileServer.Tests.Util;

public class CookieProcessingHttpMessageHandler : DelegatingHandler
{
    private CookieContainer _cookieContainer = new();

    public CookieContainer CookieContainer
    {
        get => _cookieContainer;
        set { _cookieContainer = value; }
    }

    public CookieProcessingHttpMessageHandler(HttpMessageHandler innerHandler)
        : base(innerHandler) { }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        Uri requestUri = request.RequestUri!;
        SetCookieToHeadersFromContainer(request, requestUri);
        HttpResponseMessage response = await base.SendAsync(request, ct);
        SetCookieFromHeadersToContainer(response, requestUri);
        return response;
    }

    private void SetCookieToHeadersFromContainer(HttpRequestMessage request, Uri requestUri)
    {
        string uriCookie = _cookieContainer.GetCookieHeader(requestUri);
        if (uriCookie != "")
            request.Headers.Add(HeaderNames.Cookie, uriCookie);
    }

    private void SetCookieFromHeadersToContainer(HttpResponseMessage response, Uri requestUri)
    {
        if (response.Headers.TryGetValues(HeaderNames.SetCookie, out IEnumerable<string>? setCookieHeaders))
        {
            foreach (SetCookieHeaderValue cookieHeader in SetCookieHeaderValue.ParseList(setCookieHeaders.ToList()))
            {
                Cookie cookie = new(cookieHeader.Name.Value!, cookieHeader.Value.Value, cookieHeader.Path.Value);
                if (cookieHeader.Expires.HasValue)
                    cookie.Expires = cookieHeader.Expires.Value.DateTime;
                _cookieContainer.Add(requestUri, cookie);
            }
        }
    }
}
