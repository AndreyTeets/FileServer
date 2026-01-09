namespace FileServer.E2ETests.Snapshots;

internal sealed class AfterOpenInitialPageSnapshot : Snapshot
{
    protected override string Value => """
        - paragraph: "Menu:"
        - button "DownloadPage"
        - button "UploadPage"
        - button "AuthPage"
        """;
}
