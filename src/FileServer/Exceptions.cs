namespace FileServer;

#pragma warning disable MA0048 // File name must match type name
public class StartupException(string? message, Exception? innerException) : Exception(message, innerException)
{
    public StartupException(string? message) : this(message, innerException: null) { }
    public StartupException() : this(message: null, innerException: null) { }
}

public class AuthException(string? message, Exception? innerException) : Exception(message, innerException)
{
    public AuthException(string? message) : this(message, innerException: null) { }
    public AuthException() : this(message: null, innerException: null) { }
}
#pragma warning restore MA0048 // File name must match type name
