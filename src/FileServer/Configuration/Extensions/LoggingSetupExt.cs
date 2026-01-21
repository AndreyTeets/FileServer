namespace FileServer.Configuration.Extensions;

internal static class LoggingSetupExt
{
    public static void SetUpProviders(this ILoggingBuilder builder)
    {
        builder.ClearProviders();
        builder.AddConsole();
    }
}
