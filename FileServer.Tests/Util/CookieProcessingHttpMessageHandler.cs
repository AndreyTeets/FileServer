using System.Net;
using Microsoft.Net.Http.Headers;

namespace FileServer.Tests.Util;

public class CookieProcessingHttpMessageHandler : DelegatingHandler
{
    public CookieContainer CookieContainer { get; set; } = new();

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
        string uriCookie = CookieContainer.GetCookieHeader(requestUri);
        if (uriCookie != "")
            request.Headers.Add(HeaderNames.Cookie, uriCookie);
    }

    private void SetCookieFromHeadersToContainer(HttpResponseMessage response, Uri requestUri)
    {
        if (response.Headers.TryGetValues(HeaderNames.SetCookie, out IEnumerable<string>? setCookieHeaders))
        {
            foreach (SetCookieHeaderValue cookieHeader in SetCookieHeaderValue.ParseList([.. setCookieHeaders]))
            {
                Cookie cookie = new(cookieHeader.Name.Value!, cookieHeader.Value.Value, cookieHeader.Path.Value);
                if (cookieHeader.Expires.HasValue)
                    cookie.Expires = cookieHeader.Expires.Value.DateTime;
                CookieContainer.Add(requestUri, cookie);
            }
        }
    }
}
