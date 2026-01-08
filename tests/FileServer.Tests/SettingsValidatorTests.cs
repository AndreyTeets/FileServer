using System.Reflection;
using FileServer.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FileServer.Tests;

internal sealed class SettingsValidatorTests : TestsBase
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

    [TestCase(nameof(Settings.DownloadAnonDir), null, "is null")]
    //[TestCase(nameof(Settings.DownloadAnonDir), "", "is null")]
    [TestCase(nameof(Settings.DownloadDir), null, "is null")]
    //[TestCase(nameof(Settings.DownloadDir), "", "is null")]
    [TestCase(nameof(Settings.UploadDir), null, "is null")]
    //[TestCase(nameof(Settings.UploadDir), "", "is null")]
    //[TestCase(nameof(Settings.SigningKey), null, "is null")]
    //[TestCase(nameof(Settings.SigningKey), "", "is null")]
    [TestCase(nameof(Settings.SigningKey), "1234567890123456789", "length < 20")]
    [TestCase(nameof(Settings.SigningKey), "12345678901234567890123456789012345678901234567890123456789012345", "length > 64")]
    //[TestCase(nameof(Settings.LoginKey), null, "is null")]
    //[TestCase(nameof(Settings.LoginKey), "", "is null")]
    [TestCase(nameof(Settings.LoginKey), "12345678901", "length < 12")]
    [TestCase(nameof(Settings.TokensTtlSeconds), null, "is null")]
    public async Task Validate_Fails_OnInvalidPropValue(
        string propName, object? propValue, string expectedProblem)
    {
        Settings settings = CreateValidSettings();
        PropertyInfo field = typeof(Settings).GetProperties().Single(x => x.Name == propName);
        field.SetValue(settings, propValue);
        ValidateOptionsResult res = _validator.Validate(name: null, settings);

        Assert.That(res.Failed, Is.True);
        Assert.That(res.FailureMessage, Is.EqualTo($"{propName} {expectedProblem}"));
    }

    [Test]
    public async Task Validate_FailsAndReportsAllProblems_OnUnsetProps()
    {
        ValidateOptionsResult res = _validator.Validate(name: null, new Settings()
        {
            SigningKey = "12345678901234567890",
            LoginKey = "123456789012",
        });
        List<string> expectedProblems = [.. GetSettingsValidatedPropNames().Select(x => $"{x} is null")];

        Assert.That(res.Failed, Is.True);
        Assert.That(res.Failures, Is.EquivalentTo(expectedProblems));

        static List<string> GetSettingsValidatedPropNames() =>
        [
            nameof(Settings.DownloadAnonDir),
            nameof(Settings.DownloadDir),
            nameof(Settings.UploadDir),
            //nameof(Settings.SigningKey),
            //nameof(Settings.LoginKey),
            nameof(Settings.TokensTtlSeconds),
        ];
    }

    private static Settings CreateValidSettings() => new()
    {
        DownloadAnonDir = "something_not_empty",
        DownloadDir = "something_not_empty",
        UploadDir = "something_not_empty",
        SigningKey = "12345678901234567890",
        LoginKey = "123456789012",
        TokensTtlSeconds = 1,
    };
}
