using FileServer.Configuration;
using FileServer.Configuration.Extensions;

namespace FileServer;

internal sealed class Program
{
    private static async Task Main(string[] args)
    {
        if (args.Length > 0)
        {
            AppInfo.ShowVersionAndUsage();
            return;
        }

        ILogger logger = Utility.CreateConsoleLogger<Program>();
        logger.LogInformation(LogMessages.StartingServer, AppInfo.GetVersion(out string commit), commit);

        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.ConfigureSettings();
        builder.ConfigureLogging();
        builder.ConfigureKestrel(logger);

        builder.Services.AddAndConfigureServices();

        WebApplication app = builder.Build();
        app.SetUpSettingsMonitor();

        app.UseToIndexPageRedirect();
        app.UseStaticFilesWithNoCacheHeaders();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapRoutes();
        app.UseNoCacheHeaders();

        await app.RunAsync();
    }
}
