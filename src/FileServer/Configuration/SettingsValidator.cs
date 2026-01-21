using Microsoft.Extensions.Options;

namespace FileServer.Configuration;

internal sealed class SettingsValidator(
    IServiceProvider serviceProvider,
    IDebouncer debouncer)
    : IValidateOptions<Settings>
{
    private const int MinSigningKeyLength = 20;
    private const int MaxSigningKeyLength = 64;
    private const int MinLoginKeyLength = 12;

    private readonly ILogger<Program> _logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    private readonly IDebouncer _debouncer = debouncer;

    public ValidateOptionsResult Validate(string? name, Settings settings)
    {
        if (!SettingsAreValid(settings, out List<string> problems))
        {
            _debouncer.Debounce(nameof(LogMessages.InvalidSettings), () =>
                _logger.LogError(LogMessages.InvalidSettings,
                    LogUtil.GetSettingsDisplayString(settings),
                    LogUtil.GetSettingsProblemsDisplayString(problems)));
            return ValidateOptionsResult.Fail(problems);
        }
        return ValidateOptionsResult.Success;
    }

    private static bool SettingsAreValid(Settings settings, out List<string> problems)
    {   // The address/port/cert settings aren't checked as they are used before validation is available
        problems = [];

        if (!IsSet(settings.DownloadAnonDir))
            problems.Add($"{nameof(Settings.DownloadAnonDir)} is not set");
        if (!IsSet(settings.DownloadDir))
            problems.Add($"{nameof(Settings.DownloadDir)} is not set");
        if (!IsSet(settings.UploadDir))
            problems.Add($"{nameof(Settings.UploadDir)} is not set");
        if (!IsSet(settings.SigningKey))
            problems.Add($"{nameof(Settings.SigningKey)} is not set");
        if (!IsSet(settings.LoginKey))
            problems.Add($"{nameof(Settings.LoginKey)} is not set");
        if (!IsSet(settings.TokensTtlSeconds))
            problems.Add($"{nameof(Settings.TokensTtlSeconds)} is not set");

        if (IsSet(settings.SigningKey) && settings.SigningKey.Length < MinSigningKeyLength)
            problems.Add($"{nameof(Settings.SigningKey)} length < {MinSigningKeyLength}");
        if (IsSet(settings.SigningKey) && settings.SigningKey.Length > MaxSigningKeyLength)
            problems.Add($"{nameof(Settings.SigningKey)} length > {MaxSigningKeyLength}");

        if (IsSet(settings.LoginKey) && settings.LoginKey.Length < MinLoginKeyLength)
            problems.Add($"{nameof(Settings.LoginKey)} length < {MinLoginKeyLength}");

        return problems.Count == 0;
    }

    private static bool IsSet(string? setting) => !string.IsNullOrEmpty(setting);
    private static bool IsSet(int setting) => setting != int.MinValue;
}
