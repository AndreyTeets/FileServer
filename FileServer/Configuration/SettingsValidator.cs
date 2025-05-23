﻿using Microsoft.Extensions.Options;

namespace FileServer.Configuration;

internal sealed class SettingsValidator(
    IServiceProvider serviceProvider,
    IDebouncer debouncer)
    : IValidateOptions<Settings>
{
    private const int MinLoginKeyLength = 12;
    private const int MinSigningKeyLength = 20;
    private const int MaxSigningKeyLength = 64;

    private readonly ILogger<Program> _logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    private readonly IDebouncer _debouncer = debouncer;

    public ValidateOptionsResult Validate(string? name, Settings settings)
    {
        if (!SettingsAreValid(settings, out List<string> problems))
        {
            _debouncer.Debounce(nameof(LogMessages.InvalidSettings), () =>
                _logger.LogError(LogMessages.InvalidSettings,
                    Utility.GetSettingsDisplayString(settings),
                    Utility.GetSettingsProblemsDisplayString(problems)));
            return ValidateOptionsResult.Fail(problems);
        }
        return ValidateOptionsResult.Success;
    }

    private static bool SettingsAreValid(Settings settings, out List<string> problems)
    {
        problems = [];

        if (settings.DownloadAnonDir is null)
            problems.Add($"{nameof(Settings.DownloadAnonDir)} is null");
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

        return problems.Count == 0;
    }
}
