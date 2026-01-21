using System.Diagnostics.CodeAnalysis;
using System.Threading.RateLimiting;
using FileServer.Auth;
using FileServer.Routes;
using FileServer.Services;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Options;

namespace FileServer.Configuration.Extensions;

internal static class RoutingServicesSetupExt
{
    public static void SetUpForRouting(this IServiceCollection services)
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
        services.AddAllRouteHandlers();
    }

    [UnconditionalSuppressMessage("Trimming", "IL2077",
        Justification = "All types in FileServer.Routes namespace are excluded from trimming in TrimConfig")]
    private static void AddAllRouteHandlers(this IServiceCollection services)
    {
        foreach ((Type interfaceType, Type implementingType) in RouteHandlersLocator.GetAll())
            services.AddTransient(interfaceType, implementingType);
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
