namespace FileServer;

#pragma warning disable MA0048 // File name must match type name
internal sealed class StartupException(string? message, Exception? innerException) : Exception(message, innerException)
{
    public StartupException(string? message) : this(message, innerException: null) { }
    public StartupException() : this(message: null, innerException: null) { }
}

internal sealed class AuthException(string? message, Exception? innerException) : Exception(message, innerException)
{
    public AuthException(string? message) : this(message, innerException: null) { }
    public AuthException() : this(message: null, innerException: null) { }
}
