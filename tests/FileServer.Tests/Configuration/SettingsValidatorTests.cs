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

    [Test]
    public async Task Validate_Succeeds_WhenCorrectSettings()
    {
        ValidateOptionsResult res = _validator.Validate(name: null, CreateValidSettings());
        Assert.That(res.Succeeded, Is.True);
    }

    [TestCase(nameof(Settings.ListenAddress), null, "is not set")]
    [TestCase(nameof(Settings.ListenAddress), "", "is not set")]
    [TestCase(nameof(Settings.ListenAddress), "x", "value is not a valid ip address")]
    [TestCase(nameof(Settings.ListenPort), int.MinValue, "is not set")]
    [TestCase(nameof(Settings.ListenPort), -1, "value is not a valid port")]
    [TestCase(nameof(Settings.CertFilePath), null, "is not set")]
    [TestCase(nameof(Settings.CertFilePath), "", "is not set")]
    [TestCase(nameof(Settings.CertFilePath), "x", "value is not a full path")]
    [TestCase(nameof(Settings.CertKeyPath), null, "is not set")]
    [TestCase(nameof(Settings.CertKeyPath), "", "is not set")]
    [TestCase(nameof(Settings.CertKeyPath), "x", "value is not a full path")]
    [TestCase(nameof(Settings.DownloadAnonDir), null, "is not set")]
    [TestCase(nameof(Settings.DownloadAnonDir), "", "is not set")]
    [TestCase(nameof(Settings.DownloadDir), null, "is not set")]
    [TestCase(nameof(Settings.DownloadDir), "", "is not set")]
    [TestCase(nameof(Settings.UploadDir), null, "is not set")]
    [TestCase(nameof(Settings.UploadDir), "", "is not set")]
    [TestCase(nameof(Settings.SigningKey), null, "is not set")]
    [TestCase(nameof(Settings.SigningKey), "", "is not set")]
    [TestCase(nameof(Settings.SigningKey), "1234567890123456789", "length < 20")]
    [TestCase(nameof(Settings.SigningKey), "12345678901234567890123456789012345678901234567890123456789012345", "length > 64")]
    [TestCase(nameof(Settings.LoginKey), null, "is not set")]
    [TestCase(nameof(Settings.LoginKey), "", "is not set")]
    [TestCase(nameof(Settings.LoginKey), "12345678901", "length < 12")]
    [TestCase(nameof(Settings.TokensTtlSeconds), int.MinValue, "is not set")]
    public async Task Validate_Fails_OnInvalidPropValue(
        string propName, object? propValue, string expectedProblem)
    {
        Settings settings = CreateValidSettings();
        PropertyInfo prop = typeof(Settings).GetProperties().Single(p => p.Name == propName);
        prop.SetValue(settings, propValue);
        ValidateOptionsResult res = _validator.Validate(name: null, settings);

        Assert.That(res.Failed, Is.True);
        Assert.That(res.FailureMessage, Is.EqualTo($"{propName} {expectedProblem}"));
    }

    [Test]
    public async Task Validate_FailsAndReportsAllProblems_OnUnsetProps()
    {
        ValidateOptionsResult res = _validator.Validate(name: null, new Settings());
        List<string> expectedProblems = [.. typeof(Settings).GetProperties().Select(p => $"{p.Name} is not set")];

        Assert.That(res.Failed, Is.True);
        Assert.That(res.Failures, Is.EquivalentTo(expectedProblems));
    }

    private static Settings CreateValidSettings() => new()
    {
        ListenAddress = "0.0.0.0",
        ListenPort = 0,
        CertFilePath = Path.GetFullPath("some_path"),
        CertKeyPath = Path.GetFullPath("some_path"),
        DownloadAnonDir = "something_not_empty",
        DownloadDir = "something_not_empty",
        UploadDir = "something_not_empty",
        SigningKey = "12345678901234567890",
        LoginKey = "123456789012",
        TokensTtlSeconds = 1,
    };
}
