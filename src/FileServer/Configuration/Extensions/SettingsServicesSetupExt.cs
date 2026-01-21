using Microsoft.Extensions.Options;

namespace FileServer.Configuration.Extensions;

internal static class SettingsServicesSetupExt
{
    public static void SetUpForSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<Settings>(configuration.GetRequiredSection(nameof(Settings)));
        services.AddSingleton<IValidateOptions<Settings>, SettingsValidator>();
        services.AddSingleton<IDebouncer, Debouncer>();
    }

    public static void SetUpSettingsMonitor(this IServiceProvider services)
    {
        ILogger logger = services.GetRequiredService<ILogger<Program>>();
        IOptionsMonitor<Settings> settingsMonitor = services.GetRequiredService<IOptionsMonitor<Settings>>();
        IDebouncer debouncer = services.GetRequiredService<IDebouncer>();
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
