using System.Text;
using FileServer.Configuration;
using FileServer.Configuration.Extensions;
using FileServer.Tests.Util;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileServer.Tests;

[SetUpFixture]
internal sealed class SetUpTestServerFixture
{
    public static IHost? Host { get; private set; }
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
                    services.AddSingleton<IValidateOptions<Settings>, SettingsValidator>();
                    services.AddSingleton<IDebouncer, NoopDebouncer>();
                    services.AddAndConfigureServices();
                    services.AddRouting();
                })
                .Configure(app =>
                {
                    app.Use(async (context, next) =>
                    {
                        context.Features.Set<IHttpMaxRequestBodySizeFeature>(new FakeHttpMaxRequestBodySizeFeature());
                        await next();
                    });
                    app.UseToIndexPageRedirect();
                    app.UseStaticFilesWithNoCacheHeaders();
                    app.UseRouting();
                    app.UseAuthentication();
                    app.UseAuthorization();
                    app.UseEndpoints(endpoints =>
                        endpoints.MapRoutes(endpoints.ServiceProvider));
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

    private static Dictionary<string, string?> CreateAppSettings() => new()
    {
        { "Logging:LogLevel:Default", "Trace" },
        { "Logging:LogLevel:Microsoft.AspNetCore", "Error" },
        { "Logging:LogLevel:FileServer.Auth.DoubleTokenAuthenticationHandler", "Error" },
        { "Logging:LogLevel:Microsoft.AspNetCore.Hosting.Diagnostics", "Information" },
        { $"{nameof(Settings)}:{nameof(Settings.DownloadAnonDir)}", Path.GetFullPath("fs_data/download_anon") },
        { $"{nameof(Settings)}:{nameof(Settings.DownloadDir)}", Path.GetFullPath("fs_data/download") },
        { $"{nameof(Settings)}:{nameof(Settings.UploadDir)}", Path.GetFullPath("fs_data/upload") },
        { $"{nameof(Settings)}:{nameof(Settings.SigningKey)}", "12345678901234567890" },
        { $"{nameof(Settings)}:{nameof(Settings.LoginKey)}", "123456789012" },
        { $"{nameof(Settings)}:{nameof(Settings.TokensTtlSeconds)}", "30" },
    };

    internal sealed class NoopDebouncer : IDebouncer
    {
        public void Debounce(string category, Action action) => action();
        public void Dispose() { }
    }

    internal sealed class FakeHttpMaxRequestBodySizeFeature : IHttpMaxRequestBodySizeFeature
    {
        public bool IsReadOnly => false;
        public long? MaxRequestBodySize { get; set; }
    }
}
