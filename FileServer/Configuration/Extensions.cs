using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using FileServer.Configuration;
using Microsoft.Extensions.Options;

namespace FileServer.Configuration;

public static class Extensions
{
    public static void ConfigureSettings(this WebApplicationBuilder builder)
    {
        string settingsFilePath = Environment.GetEnvironmentVariable("FileServer_SettingsFilePath")
            ?? "appsettings.json";
        builder.Configuration.Sources.Clear();
        builder.Configuration
            .AddJsonFile(settingsFilePath, optional: false, reloadOnChange: true)
            .AddEnvironmentVariables("FileServer__");

        builder.Services.Configure<Settings>(builder.Configuration.GetSection(nameof(Settings)));
        builder.Services.AddTransient<Debouncer>();
        builder.Services.AddSingleton<IValidateOptions<Settings>, SettingsValidator>();
    }

    public static void ConfigureLogging(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
    }

    public static void ConfigureKestrel(this WebApplicationBuilder builder)
    {
        ILogger logger = Utility.CreateConsoleLogger<Program>();
        Settings settings = builder.Configuration.GetSection(nameof(Settings)).Get<Settings>()!;

        X509Certificate2 cert = Utility.LoadCertificate(settings);
        logger.LogInformation($"Using Certificate:\n{Utility.GetCertificateDisplayString(cert)}");

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Listen(IPAddress.Parse(settings.ListenAddress!), settings.ListenPort!.Value, listenOptions =>
            {
                listenOptions.UseHttps(httpsOptions =>
                {
                    httpsOptions.ServerCertificate = cert;
                    httpsOptions.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
                });
            });
        });
    }

    public static void SetupSettingsMonitor(this WebApplication app)
    {
        ILogger logger = app.Services.GetRequiredService<ILogger<Program>>();
        IOptionsMonitor<Settings> settingsMonitor = app.Services.GetRequiredService<IOptionsMonitor<Settings>>();
        Debouncer onSettingsChangeDebouncer = app.Services.GetRequiredService<Debouncer>();

        logger.LogInformation($"Using Settings:\n{Utility.GetSettingsDisplayString(settingsMonitor.CurrentValue)}");
        settingsMonitor.OnChange(settings => onSettingsChangeDebouncer.Debounce(() =>
        {
            logger.LogInformation($"Settings changed. New Settings:\n{Utility.GetSettingsDisplayString(settings)}");
        }));
    }
}
