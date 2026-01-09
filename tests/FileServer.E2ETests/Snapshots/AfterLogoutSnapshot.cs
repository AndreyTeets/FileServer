namespace FileServer.E2ETests.Snapshots;

internal sealed class AfterLogoutSnapshot : Snapshot
{
    protected override string Value => """
        - paragraph: "Menu:"
        - button "DownloadPage"
        - button "UploadPage"
        - button "AuthPage"
        - paragraph: "AuthPage:"
        - text: "Key:"
        - textbox: "123456789012"
        - button "Login"
        """;
}
