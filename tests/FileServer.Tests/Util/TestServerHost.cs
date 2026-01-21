using System.Text;
using FileServer.Configuration;
using FileServer.Configuration.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FileServer.Tests.Util;

internal static class TestServerHost
{
    public static IHost Create(StringBuilder logsSb)
    {
        WebApplicationBuilder builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions());
        builder.Configuration.UseTestSource();
        builder.Logging.UseTestProvider(builder.Configuration, logsSb);
        builder.Services.SetUpForSettingsWithNoopDebouncer(builder.Configuration);
        builder.Services.SetUpForRouting();
        builder.Services.AddRouting(); // Need to add manually because CreateEmptyBuilder is used
        builder.WebHost.UseTestServer();

        WebApplication app = builder.Build();
        app.Services.SetUpSettingsMonitor();
        app.UseTestFeatures(); // TestServer doesn't have all features that Kestrel does
        app.SetUpRouting();

        return app;
    }

    private static void UseTestSource(this IConfigurationManager configuration) =>
        configuration.AddInMemoryCollection(CreateTestAppSettings());

    private static void UseTestProvider(
        this ILoggingBuilder builder,
        IConfiguration configuration,
        StringBuilder logsSb)
    {
        builder.AddConfiguration(configuration.GetRequiredSection("Logging"));
        builder.Services.AddSingleton<ILoggerProvider, StringBuilderLoggerProvider>(
            sp => new StringBuilderLoggerProvider(logsSb));
    }

    public static void SetUpForSettingsWithNoopDebouncer(this IServiceCollection services, IConfiguration configuration)
    {
        services.SetUpForSettings(configuration);
        services.RemoveAll<IDebouncer>();
        services.AddSingleton<IDebouncer, NoopDebouncer>();
    }

    private static void UseTestFeatures(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            context.Features.Set<IHttpMaxRequestBodySizeFeature>(new FakeHttpMaxRequestBodySizeFeature());
            await next();
        });
    }

    private static Dictionary<string, string?> CreateTestAppSettings() => new()
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
