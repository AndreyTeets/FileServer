using FileServer.Configuration;

namespace FileServer;

internal sealed class Program
{
    private static async Task Main(string[] args)
    {
        if (args.Length > 0)
        {
            Utility.ShowVersionAndUsage();
            return;
        }

        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.ConfigureSettings();
        builder.ConfigureLogging();
        builder.ConfigureKestrel();

        builder.Services.AddAndConfigureServices();

        WebApplication app = builder.Build();
        app.SetupSettingsMonitor();

        app.UseToIndexPageRedirect();
        app.UseStaticFilesWithNoCacheHeaders();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseControllersWithAuthorization();
        app.UseNoCacheHeaders();

        await app.RunAsync();
    }
}
