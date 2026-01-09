namespace FileServer.E2ETests.Snapshots;

internal sealed class AfterOpenDownloadPageNoAuthSnapshot : Snapshot
{
    protected override string Value => """
        - paragraph: "Menu:"
        - button "DownloadPage"
        - button "UploadPage"
        - button "AuthPage"
        - paragraph: "DownloadPage:"
        - table:
          - rowgroup:
            - row "Anon Path Size":
              - cell "Anon"
              - cell "Path"
              - cell "Size"
          - rowgroup:
            - row "yes anonfile1.txt 22 Download View":
              - cell "yes"
              - cell "anonfile1.txt"
              - cell "22"
              - cell "Download":
                - button "Download"
              - cell "View":
                - button "View"
        """;
}
