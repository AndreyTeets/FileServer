using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.RateLimiting;
using FileServer.Auth;
using FileServer.Routes;
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
        services.AddTransient<FileSaver>();
        services.AddTransient<FilesLister>();
        services.AddTransient<TokenService>();

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

        services.AddPerRouteAndIpRateLimiter();
        services.AddHttpContextAccessor();
        AddAllRouteHandlers(services);

        [UnconditionalSuppressMessage("Trimming", "IL2077",
            Justification = "All types in FileServer.Routes namespace are excluded from trimming in TrimConfig")]
        static void AddAllRouteHandlers(IServiceCollection services)
        {
            foreach ((Type interfaceType, Type implementingType) in RouteHandlersLocator.GetAll())
                services.AddTransient(interfaceType, implementingType);
        }
    }

    private static void AddPerRouteAndIpRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(Constants.PerRouteAndIpRateLimitPolicyName, context =>
            {
                string route = context.GetEndpoint()!.Metadata.GetMetadata<RouteNameMetadata>()!.RouteName!;
                string ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetTokenBucketLimiter($"{route}:{ip}",
                    partitionKey => StaticSettings.GetRateLimiterOptions(GetRoute(partitionKey)));
                static string GetRoute(string pk) => pk[0..pk.IndexOf(":")];
            });
        });
    }
}
