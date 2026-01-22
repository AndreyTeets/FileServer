using System.Net;
using Microsoft.Extensions.Options;

namespace FileServer.Configuration;

internal sealed class SettingsValidator(
    ILogger<Program> logger,
    IDebouncer debouncer)
    : IValidateOptions<Settings>
{
    private const int MinSigningKeyLength = 20;
    private const int MaxSigningKeyLength = 64;
    private const int MinLoginKeyLength = 12;

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

    private static bool SettingsAreValid(Settings settings, out List<string> problems)
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

        if (IsSet(settings.SigningKey) && settings.SigningKey.Length < MinSigningKeyLength)
            problems.Add($"{nameof(Settings.SigningKey)} length < {MinSigningKeyLength}");
        if (IsSet(settings.SigningKey) && settings.SigningKey.Length > MaxSigningKeyLength)
            problems.Add($"{nameof(Settings.SigningKey)} length > {MaxSigningKeyLength}");

        if (IsSet(settings.LoginKey) && settings.LoginKey.Length < MinLoginKeyLength)
            problems.Add($"{nameof(Settings.LoginKey)} length < {MinLoginKeyLength}");

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

    private static bool IsSet(string? setting) => !string.IsNullOrEmpty(setting);
    private static bool IsSet(int setting) => setting != int.MinValue;
}
