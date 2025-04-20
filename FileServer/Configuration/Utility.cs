using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace FileServer.Configuration;

public static class Utility
{
    public static ILogger CreateConsoleLogger<T>() where T : class
    {
        using ILoggerFactory loggerFactory = LoggerFactory.Create(loggerBuilder => loggerBuilder
            .SetMinimumLevel(LogLevel.Trace)
            .AddConsole());
        return loggerFactory.CreateLogger<T>();
    }

    public static X509Certificate2 LoadCertificate(Settings settings)
    {
        X509Certificate2 cert = X509Certificate2.CreateFromPemFile(settings.CertFilePath!, settings.CertKeyPath);
        if (OperatingSystem.IsWindows())
        {
            using X509Certificate2 originalCert = cert;
            cert = new X509Certificate2(originalCert.Export(X509ContentType.Pkcs12));
        }
        return cert;
    }

    public static string GetCertificateDisplayString(X509Certificate2 cert)
    {
        StringBuilder sb = new();
        return sb
            .AppendLine($"-Subject: {cert.Subject}")
            .AppendLine($"-Issuer: {cert.Issuer}")
            .AppendLine($"-ValidFrom: {FormatDate(cert.NotBefore)}")
            .AppendLine($"-ValidTo: {FormatDate(cert.NotAfter)}")
            .AppendLine($"-SHA256: {GetSha256(cert)}")
            .AppendLine($"-SHA1: {GetSha1(cert)}")
            .ToString().Trim();

        static string GetSha256(X509Certificate2 cert)
        {
            byte[] bytes = new byte[32];
            cert.TryGetCertHash(System.Security.Cryptography.HashAlgorithmName.SHA256, bytes, out int _);
            return BitConverter.ToString(bytes);
        }

        static string GetSha1(X509Certificate2 cert)
        {
            byte[] bytes = new byte[20];
            cert.TryGetCertHash(System.Security.Cryptography.HashAlgorithmName.SHA1, bytes, out int _);
            return BitConverter.ToString(bytes);
        }

        static string FormatDate(DateTime date) =>
            date.ToString("yyyy-MM-dd HH:mm:ss");
    }

    public static string GetSettingsDisplayString(Settings settings)
    {
        StringBuilder sb = new();
        return sb
            .AppendLine($"-{nameof(Settings.ListenAddress)}: {settings.ListenAddress}")
            .AppendLine($"-{nameof(Settings.ListenPort)}: {settings.ListenPort}")
            .AppendLine($"-{nameof(Settings.CertFilePath)}: {settings.CertFilePath}")
            .AppendLine($"-{nameof(Settings.CertKeyPath)}: {settings.CertKeyPath}")
            .AppendLine($"-{nameof(Settings.DownloadAnonDir)}: {settings.DownloadAnonDir}")
            .AppendLine($"-{nameof(Settings.DownloadDir)}: {settings.DownloadDir}")
            .AppendLine($"-{nameof(Settings.UploadDir)}: {settings.UploadDir}")
            .AppendLine($"-{nameof(Settings.SigningKey)}: {DisplayKey(settings.SigningKey)}")
            .AppendLine($"-{nameof(Settings.LoginKey)}: {DisplayKey(settings.LoginKey)}")
            .AppendLine($"-{nameof(Settings.TokensTtlSeconds)}: {settings.TokensTtlSeconds}")
            .ToString().Trim();

        static string DisplayKey(string? key) =>
            string.IsNullOrWhiteSpace(key) ? "EMPTY" : "*****";
    }

    public static string GetSettingsProblemsDisplayString(IEnumerable<string> problems) =>
        $"-{string.Join($"{Environment.NewLine}-", problems)}";

    public static void ShowVersionAndUsage()
    {
        string? version = typeof(Program).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion
            ?.Split("+")[0];

#pragma warning disable MA0136 // Raw String contains an implicit end of line character
        string msg = $"""
            Version: {version}
            Usage:
                <no arguments>                Start the server.
                <any other argument(s)>       Show version and usage and exit.
            """;
#pragma warning restore MA0136 // Raw String contains an implicit end of line character

        msg = msg.Replace("\r", "").Replace("\n", Environment.NewLine);
        Console.WriteLine(msg);
    }
}
