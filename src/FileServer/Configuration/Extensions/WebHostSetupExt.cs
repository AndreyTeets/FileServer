using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace FileServer.Configuration.Extensions;

internal static class WebHostSetupExt
{
    public static void SetUpKestrel(this IWebHostBuilder builder, IConfiguration configuration, ILogger logger)
    {
        builder.ConfigureKestrel(options =>
        {
            Settings settings = configuration.GetSection(nameof(Settings)).Get<Settings>()!;

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
}
