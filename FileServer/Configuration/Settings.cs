namespace FileServer.Configuration;

public class Settings
{
    public string? ListenAddress { get; set; }
    public int? ListenPort { get; set; }
    public string? CertFilePath { get; set; }
    public string? CertKeyPath { get; set; }
    public string? DownloadAnonDir { get; set; }
    public string? DownloadDir { get; set; }
    public string? UploadDir { get; set; }
    public string? SigningKey { get; set; }
    public string? LoginKey { get; set; }
    public int? TokensTtlSeconds { get; set; }
}
