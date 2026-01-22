using Microsoft.Extensions.Options;

namespace FileServer.Configuration.Extensions;

internal static class SettingsServicesSetupExt
{
    public static void SetUpForSettings(
        this IServiceCollection services,
        IConfiguration configuration,
        ILogger<Program> logger,
        out Settings settings)
    {
        IConfigurationSection settingsSection = configuration.GetRequiredSection(nameof(Settings));
        settings = settingsSection.Get<Settings>()!;
        EnsureSettingsAreValid(settings, logger);
        logger.LogInformation(LogMessages.UsingSettings, LogUtil.GetSettingsDisplayString(settings));

        services.Configure<Settings>(settingsSection);
        services.AddSingleton<IValidateOptions<Settings>, SettingsValidator>();
        services.AddSingleton<IDebouncer, Debouncer>();
    }

    public static void SetUpSettingsMonitor(this IServiceProvider services)
    {
        IOptionsMonitor<Settings> settingsMonitor = services.GetRequiredService<IOptionsMonitor<Settings>>();
        ILogger logger = services.GetRequiredService<ILogger<Program>>();
        IDebouncer debouncer = services.GetRequiredService<IDebouncer>();

        settingsMonitor.OnChange(settings => debouncer.Debounce(nameof(LogMessages.SettingsChanged), () =>
            logger.LogInformation(LogMessages.SettingsChanged, LogUtil.GetSettingsDisplayString(settings))));
    }

    private static void EnsureSettingsAreValid(Settings settings, ILogger<Program> logger)
    {
        using Debouncer debouncer = new(waitTime: TimeSpan.Zero);
        SettingsValidator validator = new(logger, debouncer);
        ValidateOptionsResult validationResult = validator.Validate(name: null, settings);
        if (validationResult.Failed)
            throw new OptionsValidationException("", typeof(Settings), validationResult.Failures);
    }
}
