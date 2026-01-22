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

        ILogger<Program> logger = LogUtil.CreateConsoleLogger<Program>();
        logger.LogInformation(LogMessages.StartingServer, AppInfo.GetVersion(out string commit), commit);

        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Configuration.SetUpSources();
        builder.Logging.SetUpProviders();
        builder.Services.SetUpForSettings(builder.Configuration, logger, out Settings settings);
        builder.Services.SetUpForRouting();
        builder.WebHost.SetUpKestrel(settings, logger);

        WebApplication app = builder.Build();
        app.Services.SetUpSettingsMonitor();
        app.SetUpRouting();

        await app.RunAsync();
    }
}
