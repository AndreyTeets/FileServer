using System.Reflection;
using System.Text;
using FileServer.Configuration;

namespace FileServer.Tests.Configuration;

internal sealed class LogUtilTests
{
    [TestCase(ValuesType.IfNullableNullElseDefault)]
    [TestCase(ValuesType.Default)]
    [TestCase(ValuesType.Filled)]
    public async Task GetSettingsDisplayString_DisplaysValuesCorrectly(ValuesType valuesType)
    {
        Settings settings = new();
        StringBuilder expectedDisplayStringSb = new();
        foreach (PropertyInfo prop in typeof(Settings).GetProperties())
        {
            prop.SetValue(settings, GetTestValue(prop, valuesType, @default: prop.GetValue(settings)));
            expectedDisplayStringSb.AppendLine($"-{prop.Name}: {GetExpectedDisplayStr(prop, valuesType)}");
        }

        string displayStr = LogUtil.GetSettingsDisplayString(settings);
        Assert.That(displayStr, Is.EqualTo(expectedDisplayStringSb.ToString().Trim()));

        static object? GetTestValue(PropertyInfo prop, ValuesType valuesType, object? @default) => valuesType switch
        {
            ValuesType.IfNullableNullElseDefault when prop.PropertyType == typeof(string) => null,
            ValuesType.IfNullableNullElseDefault when prop.PropertyType == typeof(int) => @default,
            ValuesType.Default => @default,
            ValuesType.Filled => GetFilledTestValue(prop),
            _ => throw new ArgumentOutOfRangeException(nameof(valuesType), $"Invalid values type '{valuesType}'."),
        };

        static string GetExpectedDisplayStr(PropertyInfo prop, ValuesType valuesType) => valuesType switch
        {
            ValuesType.IfNullableNullElseDefault or ValuesType.Default when PropIsKey(prop) => "EMPTY",
            ValuesType.IfNullableNullElseDefault when prop.PropertyType == typeof(string) => string.Empty,
            ValuesType.IfNullableNullElseDefault when prop.PropertyType == typeof(int) => int.MinValue.ToString(),
            ValuesType.Default when prop.PropertyType == typeof(string) => string.Empty,
            ValuesType.Default when prop.PropertyType == typeof(int) => int.MinValue.ToString(),
            ValuesType.Filled when PropIsKey(prop) => "*****",
            ValuesType.Filled => $"{GetFilledTestValue(prop)}",
            _ => throw new ArgumentOutOfRangeException(nameof(valuesType), $"Invalid values type '{valuesType}'."),
        };

        static bool PropIsKey(PropertyInfo prop) => new List<string>()
        {
            nameof(Settings.SigningKey),
            nameof(Settings.LoginKey),
        }.Contains(prop.Name);
    }

    private static object GetFilledTestValue(PropertyInfo prop) => prop.PropertyType switch
    {
        Type when prop.PropertyType == typeof(string) => "some_value",
        Type when prop.PropertyType == typeof(int) => 0,
        _ => throw new ArgumentException($"Invalid property type '{prop.PropertyType}'.", nameof(prop)),
    };

    internal enum ValuesType
    {
        IfNullableNullElseDefault = 0,
        Default = 1,
        Filled = 2,
    }
}
