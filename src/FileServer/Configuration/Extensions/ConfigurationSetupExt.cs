namespace FileServer.Configuration.Extensions;

internal static class ConfigurationSetupExt
{
    public static void SetUpSources(this IConfigurationManager configuration)
    {
        string settingsFilePath = Environment.GetEnvironmentVariable("FileServer_SettingsFilePath")
            ?? "appsettings.json";

        configuration.Sources.Clear();
        configuration
            .AddJsonFile(settingsFilePath, optional: false, reloadOnChange: true)
            .AddEnvironmentVariables("FileServer__");
    }
}
