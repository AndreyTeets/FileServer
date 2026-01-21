using System.Text;
using FileServer.Configuration;
using FileServer.Configuration.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileServer.Tests.Util;

internal static class TestServerHost
{
    public static IHost Create(StringBuilder logsSb)
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
                    loggerBuilder.AddProvider(new StringBuilderLoggerProvider(logsSb));
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
                    app.UseRateLimiter();
                    app.UseAuthentication();
                    app.UseAuthorization();
                    app.UseEndpoints(endpoints => endpoints.MapRoutes());
                    app.UseNoCacheHeaders();
                });
        });
        return hostBuilder.Build();
    }

    private static Dictionary<string, string?> CreateAppSettings() => new()
    {
        { "Logging:LogLevel:Default", "Trace" },
        { "Logging:LogLevel:Microsoft.AspNetCore", "Warning" },
        { "Logging:LogLevel:Microsoft.AspNetCore.Hosting.Diagnostics", "Information" },
        { "Logging:LogLevel:Microsoft.Extensions.Hosting.Internal.Host", "Information" },
        { "Logging:LogLevel:FileServer.Program", "Warning" },
        { "Logging:LogLevel:FileServer.Auth.DoubleTokenAuthenticationHandler", "Warning" },
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
