using Microsoft.Extensions.Options;

namespace FileServer.Configuration;

public class SettingsValidator : IValidateOptions<Settings>
{
    private readonly ILogger<Program> _logger;

    public SettingsValidator(IServiceProvider serviceProvider)
    {
        _logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    }

    public ValidateOptionsResult Validate(string? name, Settings settings)
    {
        if (!SettingsAreValid(settings, out string? error))
        {
            _logger.LogCritical(error);
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

        error = problems.Count == 0
            ? null
            : $"Invalid Settings:\n" +
                $"{settings.GetDisplayString()}\n" +
                $"Problems:\n" +
                $"-{string.Join("\n-", problems)}\n";
        return error is null;
    }
}
