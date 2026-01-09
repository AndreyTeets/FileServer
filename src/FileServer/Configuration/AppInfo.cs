using System.Reflection;

namespace FileServer.Configuration;

internal static class AppInfo
{
    public static void ShowVersionAndUsage()
    {
#pragma warning disable MA0136 // Raw String contains an implicit end of line character
        string msg = $"""
            Version: {GetVersion(out string commit)}
            Commit: {commit}
            Usage:
                <no arguments>                Start the server.
                <any other argument(s)>       Show version and usage and exit.
            """;
#pragma warning restore MA0136 // Will be fixed by the code below

        msg = msg.Replace("\r", "").Replace("\n", Environment.NewLine);
        Console.WriteLine(msg);
    }

    public static string GetVersion(out string commit)
    {
        string[] versionParts = typeof(Program).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion.Split("+")
            ?? [$"Missing {nameof(AssemblyInformationalVersionAttribute)}"];

        string version = versionParts[0];
        commit = versionParts.Length > 1
            ? versionParts[1][..Math.Min(10, versionParts[1].Length)]
            : "NONE";
        return version;
    }
}
