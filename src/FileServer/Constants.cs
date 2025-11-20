namespace FileServer;

internal static class Constants
{
    public const string MainUserName = "Main";

    public const string DoubleTokenAuthenticationSchemeName = "DoubleToken";
    public const string DoubleTokenAuthenticationSchemeAuthenticationType = "DoubleToken";

    public const string AuthTokenCookieName = "FileServer-AuthToken";
    public const string AntiforgeryTokenHeaderName = "FileServer-AntiforgeryToken";
    public const string AntiforgeryTokenQueryParamName = "antiforgeryToken";

    public const string AuthClaimType = "Auth";
    public const string AntiforgeryClaimType = "Antiforgery";
}
