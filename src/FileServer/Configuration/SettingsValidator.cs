using System.Net;

namespace FileServer.Configuration;

internal sealed class SettingsValidator(
    ILogger<Program> logger,
    IDebouncer debouncer)
    : SettingsValidatorBase(logger, debouncer)
{
    protected override void Validate(Settings settings, List<string> problems)
    {
        Validate(nameof(Settings.ListenAddress), settings.ListenAddress, problems);
        Validate(nameof(Settings.ListenPort), settings.ListenPort, problems);
        Validate(nameof(Settings.CertFilePath), settings.CertFilePath, problems);
        Validate(nameof(Settings.CertKeyPath), settings.CertKeyPath, problems);
        Validate(nameof(Settings.DownloadAnonDir), settings.DownloadAnonDir, problems);
        Validate(nameof(Settings.DownloadDir), settings.DownloadDir, problems);
        Validate(nameof(Settings.UploadDir), settings.UploadDir, problems);
        Validate(nameof(Settings.SigningKey), settings.SigningKey, problems);
        Validate(nameof(Settings.LoginKey), settings.LoginKey, problems);
        Validate(nameof(Settings.TokensTtlSeconds), settings.TokensTtlSeconds, problems);
    }

    private static void Validate(string sn, string? sv, List<string> p) => ValidateCore(sn, sv, p);
    private static void Validate(string sn, int sv, List<string> p) => ValidateCore(sn, sv, p);
    private static void ValidateCore<T>(string settingName, T? settingValue, List<string> problems) where T : notnull
    {
        if (settingValue is null || IsDefault(settingValue))
        {
            problems.Add($"{settingName} is not set");
            return;
        }

        switch (settingName)
        {
            case nameof(Settings.ListenAddress):
                ValidateIsIpAddress(settingName, Cast<string>(settingValue), problems);
                break;
            case nameof(Settings.ListenPort):
                ValidateIsPort(settingName, Cast<int>(settingValue), problems);
                break;
            case nameof(Settings.CertFilePath):
            case nameof(Settings.CertKeyPath):
            case nameof(Settings.DownloadAnonDir):
            case nameof(Settings.DownloadDir):
            case nameof(Settings.UploadDir):
                ValidateIsFullPath(settingName, Cast<string>(settingValue), problems);
                break;
            case nameof(Settings.SigningKey):
                ValidateMinLength(settingName, Cast<string>(settingValue), 20, problems);
                ValidateMaxLength(settingName, Cast<string>(settingValue), 64, problems);
                break;
            case nameof(Settings.LoginKey):
                ValidateMinLength(settingName, Cast<string>(settingValue), 12, problems);
                break;
            case nameof(Settings.TokensTtlSeconds):
                ValidateIsPositive(settingName, Cast<int>(settingValue), problems);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(settingName), $"Invalid setting name '{settingName}'.");
        }

        static bool IsDefault(T settingValue) => settingValue switch
        {
            string sv => sv == string.Empty,
            int sv => sv == int.MinValue,
            _ => throw new ArgumentException($"Invalid setting type '{settingValue.GetType()}'.", nameof(settingValue)),
        };

        static TRes Cast<TRes>(T settingValue) => settingValue switch
        {
            TRes res => res,
            _ => throw new ArgumentException($"Invalid setting type '{settingValue.GetType()}'.", nameof(settingValue)),
        };
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
}
