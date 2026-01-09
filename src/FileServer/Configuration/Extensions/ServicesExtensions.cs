using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using FileServer.Auth;
using FileServer.Controllers;
using FileServer.Services;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Options;

namespace FileServer.Configuration.Extensions;

internal static class ServicesExtensions
{
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
}
