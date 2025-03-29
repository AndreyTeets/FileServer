using Microsoft.Extensions.Options;

namespace FileServer.Configuration;

public class SettingsValidator : IValidateOptions<Settings>
{
    private const int MinLoginKeyLength = 12;
    private const int MinSigningKeyLength = 20;
    private const int MaxSigningKeyLength = 64;

    private readonly ILogger<Program> _logger;
    private readonly IDebouncer _onSettingsValidationErrorDebouncer;

    public SettingsValidator(IServiceProvider serviceProvider, IDebouncer debouncer)
    {
        _logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        _onSettingsValidationErrorDebouncer = debouncer;
    }

    public ValidateOptionsResult Validate(string? name, Settings settings)
    {
        if (!SettingsAreValid(settings, out string? error))
        {
            _onSettingsValidationErrorDebouncer.Debounce(() => _logger.LogCritical(error));
            return ValidateOptionsResult.Fail(error!);
        }

        return ValidateOptionsResult.Success;
    }

    private static bool SettingsAreValid(Settings settings, out string? error)
    {
        List<string> problems = new();

        if (settings.DownloadDir is null)
            problems.Add($"{nameof(Settings.DownloadDir)} is null");
        if (settings.UploadDir is null)
            problems.Add($"{nameof(Settings.UploadDir)} is null");
        if (settings.LoginKey is null)
            problems.Add($"{nameof(Settings.LoginKey)} is null");
        if (settings.SigningKey is null)
            problems.Add($"{nameof(Settings.SigningKey)} is null");
        if (settings.TokensTtlSeconds is null)
            problems.Add($"{nameof(Settings.TokensTtlSeconds)} is null");

        if (settings.LoginKey!.Length < MinLoginKeyLength)
            problems.Add($"{nameof(Settings.LoginKey)} length < {MinLoginKeyLength}");
        if (settings.SigningKey!.Length < MinSigningKeyLength)
            problems.Add($"{nameof(Settings.SigningKey)} length < {MinSigningKeyLength}");
        if (settings.SigningKey!.Length > MaxSigningKeyLength)
            problems.Add($"{nameof(Settings.SigningKey)} length > {MaxSigningKeyLength}");

        error = problems.Count == 0
            ? null
            : $"Invalid Settings:{Environment.NewLine}" +
                $"{Utility.GetSettingsDisplayString(settings)}{Environment.NewLine}" +
                $"Problems:{Environment.NewLine}" +
                $"-{string.Join($"{Environment.NewLine}-", problems)}{Environment.NewLine}";
        return error is null;
    }
}
