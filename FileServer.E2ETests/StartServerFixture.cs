﻿using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace FileServer.E2ETests;

[SetUpFixture]
public class StartServerFixture
{
#if DEBUG
    private const string Configuration = "Debug";
#else
    private const string Configuration = "Release";
#endif
    private const string ServerProjDir = "../../../../FileServer";
    private const int PortCheckTimoutMilliseconds = 500;
    private const int PortCheckWaitNextTryMilliseconds = 1000;
    private const int StartTimeoutSec = 15;
    private const int Port = 9443;

    private Process _process;
    public static Func<string>? GetServerOutputFunc { get; private set; }

    [OneTimeSetUp]
    public void SetUpTestServer()
    {
        CreateFilesAndFoldersRequiredByServerAndTests();
        _process = StartServerProcess(out StringBuilder serverOutput);
        GetServerOutputFunc = serverOutput.ToString;
        WaitUntilServerStarted(GetServerOutputFunc);
    }

    [OneTimeTearDown]
    public void TearDownTestServer()
    {
        _process.Kill();
        _process.Dispose();
    }

    private Process StartServerProcess(out StringBuilder serverOutput)
    {
        Process process = new();
        process.StartInfo.FileName = "dotnet";
        process.StartInfo.WorkingDirectory = Path.GetFullPath(ServerProjDir);
        process.StartInfo.Arguments = $"bin/{Configuration}/net8.0/FileServer.dll";
        SetProcessInputOutputOptions(process);

        SetProcessEnv(process);

        StringBuilder output = new();
        object outputLock = new();
        process.OutputDataReceived += (_, eventArgs) => { lock (outputLock) { output.AppendLine(eventArgs.Data); } };
        process.ErrorDataReceived += (_, eventArgs) => { lock (outputLock) { output.AppendLine(eventArgs.Data); } };
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        serverOutput = output;
        return process;
    }

    private void WaitUntilServerStarted(Func<string> getServerOutputFunc)
    {
        Stopwatch sw = new();
        sw.Start();
        while (!ServerStarted())
        {
            if (sw.ElapsedMilliseconds > StartTimeoutSec * 1000)
            {
                string serverOutput = getServerOutputFunc();
                throw new TimeoutException(
                    $"Server failed to start in {StartTimeoutSec} seconds.\n" +
                    $"ServerOutput:\n" +
                    $"{serverOutput}");
            }
            Thread.Sleep(PortCheckWaitNextTryMilliseconds);
        }
    }

    private bool ServerStarted() =>
        IsPortOpen("127.0.0.1", Port, TimeSpan.FromMilliseconds(PortCheckTimoutMilliseconds));

    private bool IsPortOpen(string host, int port, TimeSpan timeout)
    {
        try
        {
            using TcpClient client = new();
            IAsyncResult result = client.BeginConnect(host, port, null, null);
            bool success = result.AsyncWaitHandle.WaitOne(timeout);
            client.EndConnect(result);
            return success;
        }
        catch
        {
            return false;
        }
    }

    private static void SetProcessEnv(Process process)
    {
        System.Collections.Specialized.StringDictionary env = process.StartInfo.EnvironmentVariables;
        env["FileServer_SettingsFilePath"] = Path.GetFullPath($"{ServerProjDir}/bin/e2etests/settings/appsettings.json");
        env["FileServer__Settings__ListenAddress"] = "127.0.0.1";
        env["FileServer__Settings__ListenPort"] = Port.ToString();
        env["FileServer__Settings__CertFilePath"] = Path.GetFullPath($"{ServerProjDir}/bin/e2etests/settings/cert.crt");
        env["FileServer__Settings__CertKeyPath"] = Path.GetFullPath($"{ServerProjDir}/bin/e2etests/settings/cert.key");
        env["FileServer__Settings__DownloadAnonDir"] = Path.GetFullPath($"{ServerProjDir}/bin/e2etests/fs_data/download_anon");
        env["FileServer__Settings__DownloadDir"] = Path.GetFullPath($"{ServerProjDir}/bin/e2etests/fs_data/download");
        env["FileServer__Settings__UploadDir"] = Path.GetFullPath($"{ServerProjDir}/bin/e2etests/fs_data/upload");
        env["FileServer__Settings__SigningKey"] = "01234567890123456789";
        env["FileServer__Settings__LoginKey"] = "012345678912";
    }

    private static void CreateFilesAndFoldersRequiredByServerAndTests()
    {
        if (Directory.Exists($"{ServerProjDir}/bin/e2etests"))
            Directory.Delete($"{ServerProjDir}/bin/e2etests", true);
        Directory.CreateDirectory($"{ServerProjDir}/bin/e2etests/settings");
        File.Copy($"{ServerProjDir}/appsettings.template.json", $"{ServerProjDir}/bin/e2etests/settings/appsettings.json");
        GenerateCertInDir(Path.GetFullPath($"{ServerProjDir}/bin/e2etests/settings"));

        Directory.CreateDirectory($"{ServerProjDir}/bin/e2etests/fs_data/download_anon");
        Directory.CreateDirectory($"{ServerProjDir}/bin/e2etests/fs_data/download");
        Directory.CreateDirectory($"{ServerProjDir}/bin/e2etests/fs_data/upload");
        File.WriteAllText($"{ServerProjDir}/bin/e2etests/fs_data/download_anon/anonfile1.txt", "test_anonfile1_content");
        File.WriteAllText($"{ServerProjDir}/bin/e2etests/fs_data/download/file1.txt", "test_file1_content");

        File.WriteAllText("test_upload_file.txt", "test_upload_file_content");
    }

    private static void GenerateCertInDir(string dir)
    {
        Process process = new();
        process.StartInfo.FileName = "openssl";
        process.StartInfo.WorkingDirectory = dir;
        process.StartInfo.Arguments = @"req -x509 -newkey rsa:2048 -sha256 -days 1 -nodes -keyout cert.key -out cert.crt -subj ""/CN=localhost""";
        SetProcessInputOutputOptions(process);
        process.Start();
        process.WaitForExit();
    }

    private static void SetProcessInputOutputOptions(Process process)
    {
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
    }
}
