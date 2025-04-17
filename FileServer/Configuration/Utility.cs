using System.Reflection;
using System.Runtime.InteropServices;
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
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            X509Certificate2 originalCert = cert;
            cert = new X509Certificate2(originalCert.Export(X509ContentType.Pkcs12));
            originalCert.Dispose();
        }
        return cert;
    }

    public static string GetCertificateDisplayString(X509Certificate2 cert)
    {
        StringBuilder sb = new();
        sb.Append($"-Subject: {cert.Subject}").AppendLine();
        sb.Append($"-Issuer: {cert.Issuer}").AppendLine();
        sb.Append($"-ValidFrom: {FormatDate(cert.NotBefore)}").AppendLine();
        sb.Append($"-ValidTo: {FormatDate(cert.NotAfter)}").AppendLine();
        sb.Append($"-SHA256: {GetSha256(cert)}").AppendLine();
        sb.Append($"-SHA1: {GetSha1(cert)}").AppendLine();
        return sb.ToString().Trim();

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

        static string FormatDate(DateTime date)
        {
            return date.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }

    public static string GetSettingsDisplayString(Settings settings)
    {
        StringBuilder sb = new();
        sb.Append($"-{nameof(Settings.ListenAddress)}: {settings.ListenAddress}").AppendLine();
        sb.Append($"-{nameof(Settings.ListenPort)}: {settings.ListenPort}").AppendLine();
        sb.Append($"-{nameof(Settings.CertFilePath)}: {settings.CertFilePath}").AppendLine();
        sb.Append($"-{nameof(Settings.CertKeyPath)}: {settings.CertKeyPath}").AppendLine();
        sb.Append($"-{nameof(Settings.DownloadAnonDir)}: {settings.DownloadAnonDir}").AppendLine();
        sb.Append($"-{nameof(Settings.DownloadDir)}: {settings.DownloadDir}").AppendLine();
        sb.Append($"-{nameof(Settings.UploadDir)}: {settings.UploadDir}").AppendLine();
        sb.Append($"-{nameof(Settings.SigningKey)}: {DisplayKey(settings.SigningKey)}").AppendLine();
        sb.Append($"-{nameof(Settings.LoginKey)}: {DisplayKey(settings.LoginKey)}").AppendLine();
        sb.Append($"-{nameof(Settings.TokensTtlSeconds)}: {settings.TokensTtlSeconds}").AppendLine();
        return sb.ToString().Trim();

        static string DisplayKey(string? key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return "EMPTY";
            return "*****";
        }
    }

    public static void ShowVersionAndUsage()
    {
        string? version = typeof(Program).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion
            ?.Split("+")[0];

        string msg = $"""
            Version: {version}
            Usage:
                <no arguments>                Start the server.
                <any other argument(s)>       Show version and usage and exit.
            """;

        msg = msg.Replace("\r", "").Replace("\n", Environment.NewLine);
        Console.WriteLine(msg);
    }
}
