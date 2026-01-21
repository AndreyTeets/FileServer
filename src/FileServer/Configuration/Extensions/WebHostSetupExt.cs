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

            X509Certificate2 cert = LoadCertificate(settings);
            logger.LogInformation(LogMessages.UsingCertificate, LogUtil.GetCertificateDisplayString(cert));

            options.Listen(IPAddress.Parse(settings.ListenAddress), settings.ListenPort, listenOptions =>
                listenOptions.UseHttps(httpsOptions =>
                {
                    httpsOptions.ServerCertificate = cert;
                    httpsOptions.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
                }));
        });
    }

    private static X509Certificate2 LoadCertificate(Settings settings)
    {
        X509Certificate2 cert = X509Certificate2.CreateFromPemFile(settings.CertFilePath, settings.CertKeyPath);
        if (OperatingSystem.IsWindows())
        {   // Windows SCHANNEL doesn't work with EphemeralKeySet (see https://github.com/dotnet/runtime/issues/23749)
            using X509Certificate2 originalCert = cert;
            cert = X509CertificateLoader.LoadPkcs12(originalCert.Export(X509ContentType.Pkcs12), password: null);
        }
        return cert;
    }
}
