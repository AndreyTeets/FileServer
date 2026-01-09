using System.Net;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using FileServer.Auth;
using FileServer.Configuration;
using FileServer.Controllers;
using FileServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace FileServer.Configuration;

internal static class Extensions
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
        builder.Services.AddSingleton<IValidateOptions<Settings>, SettingsValidator>();
        builder.Services.AddSingleton<IDebouncer, Debouncer>();
    }

    public static void ConfigureLogging(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
    }

    public static void ConfigureKestrel(this WebApplicationBuilder builder, ILogger logger)
    {
        builder.WebHost.ConfigureKestrel(options =>
        {
            Settings settings = builder.Configuration.GetSection(nameof(Settings)).Get<Settings>()!;

            X509Certificate2 cert = Utility.LoadCertificate(settings);
            logger.LogInformation(LogMessages.UsingCertificate, Utility.GetCertificateDisplayString(cert));

            options.Listen(IPAddress.Parse(settings.ListenAddress), settings.ListenPort, listenOptions =>
                listenOptions.UseHttps(httpsOptions =>
                {
                    httpsOptions.ServerCertificate = cert;
                    httpsOptions.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
                }));
        });
    }

    public static void AddAndConfigureServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<FileService>();
        services.AddSingleton<TokenService>();
        services.AddTransient<AuthController>();
        services.AddTransient<FilesController>();

        services.AddAuthentication()
            .AddScheme<DoubleTokenAuthenticationSchemeOptions, DoubleTokenAuthenticationHandler>(
                Constants.DoubleTokenAuthenticationSchemeName, options => { });
        services.AddAuthorization();

        services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, Jsc.Default));
        services.AddSingleton<IConfigureOptions<KeyManagementOptions>>(services =>
            new ConfigureOptions<KeyManagementOptions>(options =>
            {   // DataProtection isn't used but can't be disabled, this is just to get rid of the warnings
                options.XmlRepository = new InMemoryXmlRepository();
                options.XmlEncryptor = new InMemoryXmlRepository.NoopXmlEncryptor();
            }));
    }

    public static void SetUpSettingsMonitor(this WebApplication app)
    {
        ILogger logger = app.Services.GetRequiredService<ILogger<Program>>();
        IOptionsMonitor<Settings> settingsMonitor = app.Services.GetRequiredService<IOptionsMonitor<Settings>>();
        IDebouncer debouncer = app.Services.GetRequiredService<IDebouncer>();
        try
        {
            Settings currentSettings = settingsMonitor.CurrentValue; // Force settings validation here at startup
            logger.LogInformation(LogMessages.UsingSettings, Utility.GetSettingsDisplayString(currentSettings));
            settingsMonitor.OnChange(settings => debouncer.Debounce(nameof(LogMessages.SettingsChanged), () =>
                logger.LogInformation(LogMessages.SettingsChanged, Utility.GetSettingsDisplayString(settings))));
        }
        catch (OptionsValidationException ove)
        {
            debouncer.Dispose();
            throw new StartupException("Invalid settings during startup.", ove);
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
        StaticFileOptions sfo = new();
        Assembly assembly = typeof(Program).Assembly;
        if (UseEmbeddedStaticFiles(assembly))
            sfo.FileProvider = new EmbeddedFileProvider(assembly, $"{assembly.GetName().Name}.wwwroot");
        sfo.OnPrepareResponse = sfrContext =>
            sfrContext.Context.Response.Headers.CacheControl = "no-cache, no-store";
        app.UseStaticFiles(sfo);

        static bool UseEmbeddedStaticFiles(Assembly assembly) =>
            assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
                .FirstOrDefault(x => x.Key == "UseEmbeddedStaticFiles")
                ?.Value == "true";
    }

    public static void MapRoutes(this IEndpointRouteBuilder endpoints, IServiceProvider services)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api")
            .RequireAuthorization(
                new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(Constants.DoubleTokenAuthenticationSchemeName)
                    .RequireUserName(Constants.MainUserName)
                    .Build());

        AuthController authController = services.GetRequiredService<AuthController>();
        group.MapPost("/auth/login", authController.Login);
        group.MapPost("/auth/logout", authController.Logout);

        FilesController filesController = services.GetRequiredService<FilesController>();
        group.MapGet("/files/list", filesController.GetFilesList);
        group.MapGet("/files/downloadanon/{*filePath}", filesController.DownloadFileAnon);
        group.MapGet("/files/viewanon/{*filePath}", filesController.ViewFileAnon);
        group.MapGet("/files/download/{*filePath}", filesController.DownloadFile);
        group.MapGet("/files/view/{*filePath}", filesController.ViewFile);
        group.MapPost("/files/upload", filesController.UploadFile);
    }

    public static void UseNoCacheHeaders(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            if (context.Request.Method == HttpMethod.Get.Method)
                context.Response.Headers.CacheControl = "no-cache, no-store, private";
            await next();
        });
    }
}
