namespace FileServer.Configuration;

internal sealed class Settings
{
    public string ListenAddress { get; set; } = string.Empty;
    public int ListenPort { get; set; } = int.MinValue;
    public string CertFilePath { get; set; } = string.Empty;
    public string CertKeyPath { get; set; } = string.Empty;
    public string DownloadAnonDir { get; set; } = string.Empty;
    public string DownloadDir { get; set; } = string.Empty;
    public string UploadDir { get; set; } = string.Empty;
    public string SigningKey { get; set; } = string.Empty;
    public string LoginKey { get; set; } = string.Empty;
    public int TokensTtlSeconds { get; set; } = int.MinValue;
}
