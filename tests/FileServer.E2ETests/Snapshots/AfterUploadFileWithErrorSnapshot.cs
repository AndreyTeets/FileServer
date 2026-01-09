namespace FileServer.E2ETests.Snapshots;

internal sealed class AfterUploadFileWithErrorSnapshot : Snapshot
{
    protected override string Value => """
        - paragraph: "Menu:"
        - button "DownloadPage"
        - button "UploadPage"
        - button "AuthPage"
        - paragraph: "UploadPage:"
        - button "Choose File"
        - button "Upload"
        - button "Cancel" [disabled]
        - paragraph: "Failed to fetch: Response status: 409. Response body: \"File with name 'upl.test_upload_file.txt.oad' already exists.\""
        """;
}
