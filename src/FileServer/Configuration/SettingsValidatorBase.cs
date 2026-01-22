using Microsoft.Extensions.Options;

namespace FileServer.Configuration;

internal abstract class SettingsValidatorBase(
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

        bool SettingsAreValid(Settings settings, out List<string> problems)
        {
            problems = [];
            Validate(settings, problems);
            return problems.Count == 0;
        }
    }

    protected abstract void Validate(Settings settings, List<string> problems);
}
