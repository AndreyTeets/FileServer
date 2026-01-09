using Microsoft.AspNetCore.Authentication;

namespace FileServer.Auth;

#pragma warning disable S2094 // Classes should not be empty
internal sealed class DoubleTokenAuthenticationSchemeOptions : AuthenticationSchemeOptions;
#pragma warning restore S2094 // Need this custom class for clarity
