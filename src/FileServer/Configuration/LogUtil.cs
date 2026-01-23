using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace FileServer.Configuration;

internal static class LogUtil
{
    public static ILogger<T> CreateConsoleLogger<T>() where T : class
    {
        using ILoggerFactory loggerFactory = LoggerFactory.Create(loggerBuilder => loggerBuilder
            .SetMinimumLevel(LogLevel.Trace)
            .AddConsole());
        return loggerFactory.CreateLogger<T>();
    }

    public static string GetCertificateDisplayString(X509Certificate2 cert)
    {
        StringBuilder sb = new();
        return sb
            .AppendLine($"-Subject: \"{cert.Subject}\"")
            .AppendLine($"-Issuer: \"{cert.Issuer}\"")
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
            .AppendLine($"-{nameof(Settings.ListenAddress)}: {DisplayStr(settings.ListenAddress)}")
            .AppendLine($"-{nameof(Settings.ListenPort)}: {DisplayInt(settings.ListenPort)}")
            .AppendLine($"-{nameof(Settings.CertFilePath)}: {DisplayStr(settings.CertFilePath)}")
            .AppendLine($"-{nameof(Settings.CertKeyPath)}: {DisplayStr(settings.CertKeyPath)}")
            .AppendLine($"-{nameof(Settings.DownloadAnonDir)}: {DisplayStr(settings.DownloadAnonDir)}")
            .AppendLine($"-{nameof(Settings.DownloadDir)}: {DisplayStr(settings.DownloadDir)}")
            .AppendLine($"-{nameof(Settings.UploadDir)}: {DisplayStr(settings.UploadDir)}")
            .AppendLine($"-{nameof(Settings.SigningKey)}: {DisplayKey(settings.SigningKey)}")
            .AppendLine($"-{nameof(Settings.LoginKey)}: {DisplayKey(settings.LoginKey)}")
            .AppendLine($"-{nameof(Settings.TokensTtlSeconds)}: {DisplayInt(settings.TokensTtlSeconds)}")
            .ToString().Trim();

        static string DisplayKey(string? key) =>
            string.IsNullOrEmpty(key) ? DisplayUnset() : "*****";
        static string DisplayStr(string? str) =>
            string.IsNullOrEmpty(str) ? DisplayUnset() : @$"""{str}""";
        static string DisplayInt(int value) =>
            value == int.MinValue ? DisplayUnset() : value.ToString();
        static string DisplayUnset() => "UNSET";
    }

    public static string GetSettingsProblemsDisplayString(IEnumerable<string> problems) =>
        $"-{string.Join($"{Environment.NewLine}-", problems)}";
}
