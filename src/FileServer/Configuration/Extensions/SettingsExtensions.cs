using Microsoft.Extensions.Options;

namespace FileServer.Configuration.Extensions;

internal static class SettingsExtensions
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
        builder.Services.AddSingleton<IDebouncer, Debouncer>();
    }

    public static void SetUpSettingsMonitor(this WebApplication app)
    {
        ILogger logger = app.Services.GetRequiredService<ILogger<Program>>();
        IOptionsMonitor<Settings> settingsMonitor = app.Services.GetRequiredService<IOptionsMonitor<Settings>>();
        IDebouncer debouncer = app.Services.GetRequiredService<IDebouncer>();
        try
        {
            Settings currentSettings = settingsMonitor.CurrentValue; // Force settings validation here at startup
            logger.LogInformation(LogMessages.UsingSettings, Utility.GetSettingsDisplayString(currentSettings));
            settingsMonitor.OnChange(settings => debouncer.Debounce(nameof(LogMessages.SettingsChanged), () =>
                logger.LogInformation(LogMessages.SettingsChanged, Utility.GetSettingsDisplayString(settings))));
        }
        catch (OptionsValidationException ove)
        {
            debouncer.Dispose();
            throw new StartupException("Invalid settings during startup.", ove);
        }
    }
}
