using System.Text;
using Microsoft.Extensions.Options;

namespace FileServer.Configuration;

public static class Extensions
{
    public static void ConfigureSettings(this WebApplicationBuilder builder)
    {
        string settingsFilePath = Environment.GetEnvironmentVariable("FileServer_SettingsFilePath")
            ?? "appsettings.json";
        builder.Configuration.Sources.Clear();
        builder.Configuration
            .AddJsonFile(settingsFilePath, optional: false, reloadOnChange: true)
            .AddEnvironmentVariables("FileServer__");

        builder.Services.Configure<Settings>(builder.Configuration.GetSection(nameof(Settings)));
        builder.Services.AddSingleton<IValidateOptions<Settings>, SettingsValidator>();
    }

    public static void SetupSettingsMonitor(this WebApplication app)
    {
        ILogger logger = app.Services.GetRequiredService<ILogger<Program>>();
        IOptionsMonitor<Settings> settingsMonitor = app.Services.GetRequiredService<IOptionsMonitor<Settings>>();

        logger.LogInformation($"Using Settings:\n{settingsMonitor.CurrentValue.GetDisplayString()}");
        settingsMonitor.OnChange(settings =>
        {
            logger.LogInformation($"Settings changed. New Settings:\n{settings.GetDisplayString()}");
        });
    }

    public static string GetDisplayString(this Settings settings)
    {
        StringBuilder sb = new();
        sb.Append($"-{nameof(Settings.DownloadDir)}: {settings.DownloadDir}").Append('\n');
        sb.Append($"-{nameof(Settings.UploadDir)}: {settings.UploadDir}").Append('\n');
        return sb.ToString();
    }
}
