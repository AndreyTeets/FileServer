﻿using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using FileServer.Auth;
using FileServer.Configuration;
using FileServer.Services;
using Microsoft.AspNetCore.Authorization;
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
        builder.Services.AddSingleton<IDebouncer, Debouncer>();
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
        logger.LogInformation($"Using Certificate:{Environment.NewLine}" +
            $"{Utility.GetCertificateDisplayString(cert)}");

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

    public static void AddAndConfigureServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<FileService>();
        services.AddSingleton<TokenService>();

        services.AddAuthentication()
            .AddScheme<DoubleTokenAuthenticationSchemeOptions, DoubleTokenAuthenticationHandler>(
                Constants.DoubleTokenAuthenticationSchemeName, options => { });

        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = StaticSettings.JsonOptions.PropertyNamingPolicy;
        });
    }

    public static void SetupSettingsMonitor(this WebApplication app)
    {
        ILogger logger = app.Services.GetRequiredService<ILogger<Program>>();
        IOptionsMonitor<Settings> settingsMonitor = app.Services.GetRequiredService<IOptionsMonitor<Settings>>();
        IDebouncer debouncer = app.Services.GetRequiredService<IDebouncer>();

        try
        {
            Settings currentSettings = settingsMonitor.CurrentValue;
            logger.LogInformation($"Using Settings:{Environment.NewLine}" +
                $"{Utility.GetSettingsDisplayString(currentSettings)}");
            settingsMonitor.OnChange(settings => debouncer.Debounce("SettingsChanged", () =>
            {
                logger.LogInformation($"Settings changed. New Settings:{Environment.NewLine}" +
                    $"{Utility.GetSettingsDisplayString(settings)}");
            }));
        }
        catch (OptionsValidationException)
        {
            debouncer.Dispose();
            throw new Exception("Invalid settings during startup.");
        }
    }

    public static void UseToIndexPageRedirect(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            if (context.Request.Path.Value == "/")
                context.Request.Path = "/index.html";
            await next();
        });
    }

    public static void UseStaticFilesWithNoCacheHeaders(this IApplicationBuilder app)
    {
        app.UseStaticFiles(new StaticFileOptions()
        {
            OnPrepareResponse = sfrContext =>
            {
                sfrContext.Context.Response.Headers["Cache-Control"] = "no-cache, no-store";
            },
        });
    }

    public static void UseControllersWithAuthorization(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapControllers()
            .RequireAuthorization(
                new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(Constants.DoubleTokenAuthenticationSchemeName)
                    .RequireUserName(Constants.MainUserName)
                    .Build());
    }

    public static void UseNoCacheHeaders(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            if (context.Request.Method == HttpMethod.Get.Method)
                context.Response.Headers["Cache-Control"] = "no-cache, no-store, private";
            await next();
        });
    }
}
