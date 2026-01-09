namespace FileServer.E2ETests.Snapshots;

internal sealed class AfterLoginSnapshot : Snapshot
{
    protected override string Value => """
        - paragraph: "Menu:"
        - button "DownloadPage"
        - button "UploadPage"
        - button "AuthPage"
        - paragraph: "AuthPage:"
        - button "Logout"
        """;
}
