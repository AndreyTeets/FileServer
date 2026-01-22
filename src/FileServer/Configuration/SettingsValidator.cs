using System.Net;
using Microsoft.Extensions.Options;

namespace FileServer.Configuration;

internal sealed class SettingsValidator(
    ILogger<Program> logger,
    IDebouncer debouncer)
    : IValidateOptions<Settings>
{
    private readonly ILogger<Program> _logger = logger;
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

#pragma warning disable MA0051 // Method is too long
    private static bool SettingsAreValid(Settings settings, out List<string> problems)
#pragma warning restore MA0051 // Needs a lot of refactoring, will do later
    {
        problems = [];

        if (!IsSet(settings.ListenAddress))
            problems.Add($"{nameof(Settings.ListenAddress)} is not set");
        if (!IsSet(settings.ListenPort))
            problems.Add($"{nameof(Settings.ListenPort)} is not set");
        if (!IsSet(settings.CertFilePath))
            problems.Add($"{nameof(Settings.CertFilePath)} is not set");
        if (!IsSet(settings.CertKeyPath))
            problems.Add($"{nameof(Settings.CertKeyPath)} is not set");
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

        if (IsSet(settings.ListenAddress))
            ValidateIsIpAddress(nameof(Settings.ListenAddress), settings.ListenAddress, problems);
        if (IsSet(settings.ListenPort))
            ValidateIsPort(nameof(Settings.ListenPort), settings.ListenPort, problems);
        if (IsSet(settings.CertFilePath))
            ValidateIsFullPath(nameof(Settings.CertFilePath), settings.CertFilePath, problems);
        if (IsSet(settings.CertKeyPath))
            ValidateIsFullPath(nameof(Settings.CertKeyPath), settings.CertKeyPath, problems);

        if (IsSet(settings.DownloadAnonDir))
            ValidateIsFullPath(nameof(Settings.DownloadAnonDir), settings.DownloadAnonDir, problems);
        if (IsSet(settings.DownloadDir))
            ValidateIsFullPath(nameof(Settings.DownloadDir), settings.DownloadDir, problems);
        if (IsSet(settings.UploadDir))
            ValidateIsFullPath(nameof(Settings.UploadDir), settings.UploadDir, problems);

        if (IsSet(settings.SigningKey))
            ValidateMinLength(nameof(Settings.SigningKey), settings.SigningKey, 20, problems);
        if (IsSet(settings.SigningKey))
            ValidateMaxLength(nameof(Settings.SigningKey), settings.SigningKey, 64, problems);

        if (IsSet(settings.LoginKey))
            ValidateMinLength(nameof(Settings.LoginKey), settings.LoginKey, 12, problems);

        if (IsSet(settings.TokensTtlSeconds))
            ValidateIsPositive(nameof(Settings.TokensTtlSeconds), settings.TokensTtlSeconds, problems);

        return problems.Count == 0;
    }

    private static void ValidateIsIpAddress(string settingName, string settingValue, List<string> problems)
    {
        if (!IPAddress.IsValid(settingValue))
            problems.Add($"{settingName} value is not a valid ip address");
    }

    private static void ValidateIsPort(string settingName, int settingValue, List<string> problems)
    {
        if (settingValue is < 0 or > 65535)
            problems.Add($"{settingName} value is not a valid port");
    }

    private static void ValidateIsFullPath(string settingName, string settingValue, List<string> problems)
    {
        if (!Path.IsPathFullyQualified(settingValue))
            problems.Add($"{settingName} value is not a full path");
    }

    private static void ValidateMinLength(string settingName, string settingValue, int minLength, List<string> problems)
    {
        if (settingValue.Length < minLength)
            problems.Add($"{settingName} length < {minLength}");
    }

    private static void ValidateMaxLength(string settingName, string settingValue, int maxLength, List<string> problems)
    {
        if (settingValue.Length > maxLength)
            problems.Add($"{settingName} length > {maxLength}");
    }

    private static void ValidateIsPositive(string settingName, int settingValue, List<string> problems)
    {
        if (settingValue is <= 0)
            problems.Add($"{settingName} value is not positive");
    }

    private static bool IsSet(string? setting) => !string.IsNullOrEmpty(setting);
    private static bool IsSet(int setting) => setting != int.MinValue;
}
