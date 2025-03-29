using System.Text;
using FileServer.Configuration;
using FileServer.Tests.Util;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileServer.Tests;

[SetUpFixture]
public class SetupTestServerFixture
{
    public static IHost Host { get; private set; }
    public static StringBuilder LogsSb { get; } = new();

    [OneTimeSetUp]
    public async Task SetUpTestServer()
    {
        IHostBuilder hostBuilder = new HostBuilder();
        hostBuilder.ConfigureWebHost(webHostBuilder =>
        {
            ConfigurationBuilder configurationBuilder = new();
            configurationBuilder.AddInMemoryCollection(CreateAppSettings());
            IConfiguration configuration = configurationBuilder.Build();

            webHostBuilder
                .UseTestServer()
                .UseConfiguration(configuration)
                .ConfigureLogging(loggerBuilder =>
                {
                    loggerBuilder.AddConfiguration(configuration.GetSection("Logging"));
                    loggerBuilder.AddProvider(new StringBuilderLoggerProvider(LogsSb));
                })
                .ConfigureServices(services =>
                {
                    services.Configure<Settings>(configuration.GetSection(nameof(Settings)));
                    services.AddTransient<IDebouncer, NoopDebouncer>();
                    services.AddSingleton<IValidateOptions<Settings>, SettingsValidator>();
                    services.AddAndConfigureServices();
                    services.AddMvc().AddApplicationPart(typeof(Constants).Assembly);
                })
                .Configure(app =>
                {
                    app.UseToIndexPageRedirect();
                    app.UseStaticFilesWithNoCacheHeaders();
                    app.UseRouting();
                    app.UseAuthentication();
                    app.UseAuthorization();
                    app.UseEndpoints(endpoints => endpoints.UseControllersWithAuthorization());
                    app.UseNoCacheHeaders();
                });
        });

        Host = await hostBuilder.StartAsync();
    }

    [OneTimeTearDown]
    public void TearDownTestServer()
    {
        Host?.Dispose();
    }

    private Dictionary<string, string?> CreateAppSettings()
    {
        Dictionary<string, string?> appSettings = new()
        {
            { $"Logging:LogLevel:Default", "Trace" },
            { $"Logging:LogLevel:Microsoft.AspNetCore", "Error" },
            { $"Logging:LogLevel:FileServer.Auth.DoubleTokenAuthenticationHandler", "Error" },
            { $"Logging:LogLevel:Microsoft.AspNetCore.Hosting.Diagnostics", "Information" },
            { $"{nameof(Settings)}:{nameof(Settings.DownloadDir)}", Path.GetFullPath("fs_data/download") },
            { $"{nameof(Settings)}:{nameof(Settings.UploadDir)}", Path.GetFullPath("fs_data/upload") },
            { $"{nameof(Settings)}:{nameof(Settings.SigningKey)}", "01234567890123456789" },
            { $"{nameof(Settings)}:{nameof(Settings.LoginKey)}", "012345678912" },
            { $"{nameof(Settings)}:{nameof(Settings.TokensTtlSeconds)}", "30" },
        };
        return appSettings;
    }

    public class NoopDebouncer : IDebouncer
    {
        public void Debounce(Action action)
        {
            action();
        }
    }
}
