using System.Reflection;
using FileServer.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FileServer.Tests.Configuration;

internal sealed class SettingsValidatorTests : ServerTestsBase
{
#pragma warning disable CS8618 // Non-nullable variable must contain a non-null value when exiting constructor. Consider declaring it as nullable.
    private IValidateOptions<Settings> _validator;
#pragma warning restore CS8618 // Remove when `dotnet format` is fixed (see https://github.com/dotnet/sdk/issues/44867)

    [SetUp]
    public void SetUp()
    {
        _validator = TestServer.Services.GetRequiredService<IValidateOptions<Settings>>();
    }

    [TestCase(nameof(Settings.ListenAddress), "x", "value is not a valid ip address")]
    [TestCase(nameof(Settings.ListenPort), -1, "value is not a valid port")]
    [TestCase(nameof(Settings.CertFilePath), "x", "value is not a full path")]
    [TestCase(nameof(Settings.CertKeyPath), "x", "value is not a full path")]
    [TestCase(nameof(Settings.DownloadAnonDir), "x", "value is not a full path")]
    [TestCase(nameof(Settings.DownloadDir), "x", "value is not a full path")]
    [TestCase(nameof(Settings.UploadDir), "x", "value is not a full path")]
    [TestCase(nameof(Settings.SigningKey), "1234567890123456789", "length < 20")]
    [TestCase(nameof(Settings.SigningKey), "12345678901234567890123456789012345678901234567890123456789012345", "length > 64")]
    [TestCase(nameof(Settings.LoginKey), "12345678901", "length < 12")]
    [TestCase(nameof(Settings.TokensTtlSeconds), 0, "value is not positive")]
    public async Task Validate_Fails_OnInvalidPropValue(
        string propName, object? propValue, string expectedProblem)
    {
        Settings settings = CreateValidSettings();
        PropertyInfo prop = typeof(Settings).GetProperties().Single(p => p.Name == propName);
        prop.SetValue(settings, propValue);
        ValidateOptionsResult validationResult = _validator.Validate(name: null, settings);

        Assert.That(validationResult.Failed, Is.True);
        Assert.That(validationResult.FailureMessage, Is.EqualTo($"{propName} {expectedProblem}"));
    }

    [TestCase(nameof(Settings.ListenAddress))]
    [TestCase(nameof(Settings.CertFilePath))]
    [TestCase(nameof(Settings.CertKeyPath))]
    [TestCase(nameof(Settings.DownloadAnonDir))]
    [TestCase(nameof(Settings.DownloadDir))]
    [TestCase(nameof(Settings.UploadDir))]
    [TestCase(nameof(Settings.SigningKey))]
    [TestCase(nameof(Settings.LoginKey))]
    public async Task Validate_Fails_OnNullPropValue(string propName)
    {
        Settings settings = CreateValidSettings();
        PropertyInfo prop = typeof(Settings).GetProperties().Single(p => p.Name == propName);
        prop.SetValue(settings, value: null);
        ValidateOptionsResult validationResult = _validator.Validate(name: null, settings);

        Assert.That(validationResult.Failed, Is.True);
        Assert.That(validationResult.FailureMessage, Is.EqualTo($"{propName} is not set"));
    }

    [TestCase(nameof(Settings.ListenAddress))]
    [TestCase(nameof(Settings.ListenPort))]
    [TestCase(nameof(Settings.CertFilePath))]
    [TestCase(nameof(Settings.CertKeyPath))]
    [TestCase(nameof(Settings.DownloadAnonDir))]
    [TestCase(nameof(Settings.DownloadDir))]
    [TestCase(nameof(Settings.UploadDir))]
    [TestCase(nameof(Settings.SigningKey))]
    [TestCase(nameof(Settings.LoginKey))]
    [TestCase(nameof(Settings.TokensTtlSeconds))]
    public async Task Validate_Fails_OnUnsetPropValue(string propName)
    {
        Settings settingsDefaults = new();
        Settings settings = CreateValidSettings();
        PropertyInfo prop = typeof(Settings).GetProperties().Single(p => p.Name == propName);
        prop.SetValue(settings, prop.GetValue(settingsDefaults));
        ValidateOptionsResult validationResult = _validator.Validate(name: null, settings);

        Assert.That(validationResult.Failed, Is.True);
        Assert.That(validationResult.FailureMessage, Is.EqualTo($"{propName} is not set"));
    }

    [Test]
    public async Task Validate_FailsAndReportsAllProblems_OnUnsetProps()
    {
        ValidateOptionsResult validationResult = _validator.Validate(name: null, new Settings());
        List<string> expectedProblems = [.. typeof(Settings).GetProperties().Select(p => $"{p.Name} is not set")];

        Assert.That(validationResult.Failed, Is.True);
        Assert.That(validationResult.Failures, Is.EquivalentTo(expectedProblems));
    }

    [Test]
    public async Task Validate_Succeeds_WhenCorrectSettings()
    {
        ValidateOptionsResult validationResult = _validator.Validate(name: null, CreateValidSettings());
        Assert.That(validationResult.Succeeded, Is.True);
    }

    private static Settings CreateValidSettings() => new()
    {
        ListenAddress = "0.0.0.0",
        ListenPort = 0,
        CertFilePath = Path.GetFullPath("some_path"),
        CertKeyPath = Path.GetFullPath("some_path"),
        DownloadAnonDir = Path.GetFullPath("some_path"),
        DownloadDir = Path.GetFullPath("some_path"),
        UploadDir = Path.GetFullPath("some_path"),
        SigningKey = "12345678901234567890",
        LoginKey = "123456789012",
        TokensTtlSeconds = 1,
    };
}
